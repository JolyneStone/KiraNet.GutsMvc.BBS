using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Infrastructure
{
    public class JiebaLucene
    {
        private static object _sync = new object();

        #region 单例

        private static JiebaLucene _instance;
        private JiebaLucene() { }
        public static JiebaLucene Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new JiebaLucene();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region 初始化索引

        /// <summary>
        ///  初始化Lucene.Net索引
        /// </summary>
        public void InitIndex(GutsMvcUnitOfWork uf)
        {
            var replies = uf.ReplyRepository.GetAll().Join(uf.TopicRepository.GetAll(), x => x.TopicId, y => y.Id, (x, y) => new MoContentSearchItem
            {
                Id = x.Id,
                TopicId = y.Id,
                TopicName = x.TopicName,
                Content = x.Message,
                ReplyType = x.ReplyType,
                ReplyIndex = x.ReplyIndex,
                CreateTime = x.CreateTime.ToStandardFormatString()
            })
            .ToList();

            JiebaLucene.Instance.CreateIndex(replies);
        }

        #endregion

        #region 分词
        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string Token(string keyword)
        {
            string ret = "";
            System.IO.StringReader reader = new System.IO.StringReader(keyword);
            Lucene.Net.Analysis.TokenStream ts = Analyzer.TokenStream(keyword, reader);
            bool hasNext = ts.IncrementToken();
            Lucene.Net.Analysis.Tokenattributes.ITermAttribute ita;
            while (hasNext)
            {
                ita = ts.GetAttribute<Lucene.Net.Analysis.Tokenattributes.ITermAttribute>();
                ret += ita.Term + "|";
                hasNext = ts.IncrementToken();
            }
            ts.CloneAttributes();
            reader.Close();
            Analyzer.Close();
            return ret;
        }
        #endregion

        #region 获取writer

        public IndexWriter GetWriter()
        {
            IndexWriter writer = null;

            var isExsit = IndexReader.IndexExists(Directory_luce);
            if (IndexWriter.IsLocked(Directory_luce))
            {
                IndexWriter.Unlock(Directory_luce);
            }

            try
            {
                writer = new IndexWriter(Directory_luce, Analyzer, !isExsit, IndexWriter.MaxFieldLength.LIMITED);//false表示追加（true表示删除之前的重新写入）
            }
            catch
            {
                if (IndexWriter.IsLocked(Directory_luce))
                {
                    IndexWriter.Unlock(Directory_luce);
                }

                writer = new IndexWriter(Directory_luce, Analyzer, true, IndexWriter.MaxFieldLength.LIMITED);//false表示追加（true表示删除之前的重新写入）
            }

            return writer;
        }

        #endregion

        #region 创建索引
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="datalist"></param>
        /// <returns></returns>
        public bool CreateIndex(List<MoContentSearchItem> datalist)
        {
            IndexWriter writer = GetWriter();

            foreach (MoContentSearchItem data in datalist)
            {
                CreateIndex(writer, data);
            }

            writer.Optimize();
            writer.Dispose();
            return true;
        }

        public bool CreateIndex(IndexWriter writer, MoContentSearchItem data)
        {
            try
            {
                if (data == null) return false;
                Document doc = new Document();

                doc.Add(new Field("id", data.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field("topicid", data.TopicId.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("topicname", data.TopicName, Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("content", data.Content, Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("replyindex", data.ReplyIndex.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("replytype", data.ReplyType.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("createtime", data.CreateTime, Field.Store.YES, Field.Index.ANALYZED));

                writer.AddDocument(doc);
            }
            catch (System.IO.FileNotFoundException fnfe)
            {
                throw fnfe;
            }

            return true;
        }
        #endregion

        #region 添加索引
        public bool AddIndex(MoContentSearchItem data)
        {
            IndexWriter writer = null;
            try
            {
                writer = GetWriter();
                CreateIndex(writer, data);
            }
            catch (System.IO.FileNotFoundException fnfe)
            {
                return false;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Optimize();
                    writer.Dispose();
                }
            }

            return true;
        }

        #endregion

        #region 在title和content字段中查询数据
        /// <summary>
        /// 在topicname和content字段中查询数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<List<MoContentSearchItem>> Search(string keyword)
        {

            string[] fileds = { "topicname", "content" };//查询字段
                                                         //Stopwatch st = new Stopwatch();
                                                         //st.Start();
            QueryParser parser = null;// new QueryParser(Lucene.Net.Util.Version.LUCENE_30, field, analyzer);//一个字段查询
            parser = new MultiFieldQueryParser(Version, fileds, GetAnalyzer(keyword));//多个字段查询
            Query query = parser.Parse(keyword);
            int n = 1000;
            IndexSearcher searcher = new IndexSearcher(Directory_luce, true);//true-表示只读
            TopDocs docs = await Task.Run(() => searcher.Search(query, (Lucene.Net.Search.Filter)null, n));
            if (docs == null || docs.TotalHits == 0)
            {
                return null;
            }
            else
            {
                List<MoContentSearchItem> list = new List<MoContentSearchItem>();
                int counter = 1;
                foreach (ScoreDoc sd in docs.ScoreDocs)//遍历搜索到的结果
                {
                    try
                    {
                        Document doc = searcher.Doc(sd.Doc);
                        string id = doc.Get("id");
                        string topicid = doc.Get("topicid");
                        string content = doc.Get("content");
                        string topicname = doc.Get("topicname");
                        string replyindex = doc.Get("replyindex");
                        string replytype = doc.Get("replytype");
                        string createtime = doc.Get("createtime");

                        list.Add(new MoContentSearchItem
                        {
                            Id = Int32.Parse(id),
                            TopicId = Int32.Parse(topicid),
                            TopicName = topicname,
                            Content = content,
                            ReplyIndex = Int32.Parse(replyindex),
                            ReplyType = Enum.Parse<ReplyType>(replytype),
                            CreateTime = createtime
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    counter++;
                }

                return list;
            }
            //st.Stop();
            //Response.Write("查询时间：" + st.ElapsedMilliseconds + " 毫秒<br/>");

        }
        #endregion

        #region 在不同的分类下再根据title和content字段中查询数据(分页)
        /// <summary>
        /// 在不同的类型下再根据topicname和content字段中查询数据(分页)
        /// </summary>
        /// <param name="flag">分类,传空值查询全部</param>
        /// <param name="keyword"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="TotalCount"></param>
        /// <returns></returns>
        public async Task<ValueTuple<List<MoContentSearchItem>, int>> Search(string keyword, int PageIndex, int PageSize)
        {
            if (PageIndex < 1) PageIndex = 1;
            //Stopwatch st = new Stopwatch();
            //st.Start();

            //if (!String.IsNullOrWhiteSpace(flag))
            //{
            //    QueryParser qpflag = new QueryParser(Version, "flag", Analyzer);
            //    Query qflag = qpflag.Parse(flag);
            //    bq.Add(qflag, Occur.MUST);//与运算
            //}
            BooleanQuery bq = new BooleanQuery();

            if (keyword != "")
            {
                try
                {
                    string[] fileds = { "topicname", "content" };//查询字段
                    QueryParser parser = null;// new QueryParser(version, field, analyzer);//一个字段查询
                    parser = new MultiFieldQueryParser(Version, fileds, GetAnalyzer(keyword));//多个字段查询
                    Query queryKeyword = parser.Parse(keyword);
                    bq.Add(queryKeyword, Occur.MUST);//与运算
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exit...");
                    Environment.Exit(1);
                }
            }

            TopScoreDocCollector collector = TopScoreDocCollector.Create(PageIndex * PageSize, false);
            IndexSearcher searcher = new IndexSearcher(Directory_luce, true);//true-表示只读
            await Task.Run(() => searcher.Search(bq, collector));
            if (collector == null || collector.TotalHits == 0)
            {
                return (null, 0);
            }
            else
            {
                int start = PageSize * (PageIndex - 1);
                //结束数
                int limit = PageSize;
                ScoreDoc[] hits = collector.TopDocs(start, limit).ScoreDocs;
                List<MoContentSearchItem> list = new List<MoContentSearchItem>();
                int counter = 1;
                foreach (ScoreDoc sd in hits)//遍历搜索到的结果
                {
                    try
                    {
                        Document doc = searcher.Doc(sd.Doc);
                        string id = doc.Get("id");
                        string topicid = doc.Get("topicid");
                        string topicname = doc.Get("topicname");
                        string content = doc.Get("content");
                        string replyindex = doc.Get("replyindex");
                        string replytype = doc.Get("replytype");
                        string createtime = doc.Get("createtime");

                        list.Add(new MoContentSearchItem
                        {
                            Id = Int32.Parse(id),
                            TopicId = Int32.Parse(topicid),
                            TopicName = topicname,
                            Content = content,
                            ReplyIndex = Int32.Parse(replyindex),
                            ReplyType = Enum.Parse<ReplyType>(replytype),
                            CreateTime = createtime
                        });
                    }
                    catch (Exception ex)
                    {
                        return (null, 0);
                    }

                    counter++;
                }

                return (list, collector.TotalHits);
            }
            //st.Stop();
            //Response.Write("查询时间：" + st.ElapsedMilliseconds + " 毫秒<br/>");

        }
        #endregion

        #region 删除索引数据（根据id）
        /// <summary>
        /// 删除索引数据（根据id）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(string id)
        {
            bool IsSuccess = false;
            Term term = new Term("id", id);
            //Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            //Version version = new Version();
            //MultiFieldQueryParser parser = new MultiFieldQueryParser(version, new string[] { "name", "job" }, analyzer);//多个字段查询
            //Query query = parser.Parse("小王");

            //IndexReader reader = IndexReader.Open(directory_luce, false);
            //reader.DeleteDocuments(term);
            //Response.Write("删除记录结果： " + reader.HasDeletions + "<br/>");
            //reader.Dispose();

            IndexWriter writer = new IndexWriter(Directory_luce, Analyzer, false, IndexWriter.MaxFieldLength.LIMITED);
            writer.DeleteDocuments(term); // writer.DeleteDocuments(term)或者writer.DeleteDocuments(query);
                                          ////writer.DeleteAll();
            writer.Commit();
            //writer.Optimize();//
            IsSuccess = writer.HasDeletions();
            writer.Dispose();
            return IsSuccess;
        }
        #endregion

        #region 删除全部索引数据
        /// <summary>
        /// 删除全部索引数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            bool IsSuccess = true;
            try
            {
                IndexWriter writer = new IndexWriter(Directory_luce, Analyzer, false, IndexWriter.MaxFieldLength.LIMITED);
                writer.DeleteAll();
                writer.Commit();
                //writer.Optimize();//
                IsSuccess = writer.HasDeletions();
                writer.Dispose();
            }
            catch
            {
                IsSuccess = false;
            }
            return IsSuccess;
        }
        #endregion

        #region directory_luce
        private Lucene.Net.Store.Directory _directory_luce = null;
        /// <summary>
        /// Lucene.Net的目录-参数
        /// </summary>
        public Lucene.Net.Store.Directory Directory_luce
        {
            get
            {
                if (_directory_luce == null) _directory_luce = Lucene.Net.Store.FSDirectory.Open(Directory);
                return _directory_luce;
            }
        }
        #endregion

        #region directory
        private System.IO.DirectoryInfo _directory = null;
        /// <summary>
        /// 索引在硬盘上的目录
        /// </summary>
        public System.IO.DirectoryInfo Directory
        {
            get
            {
                if (_directory == null)
                {
                    string dirPath = AppDomain.CurrentDomain.BaseDirectory + "SearchIndex";
                    if (System.IO.Directory.Exists(dirPath) == false) _directory = System.IO.Directory.CreateDirectory(dirPath);
                    else _directory = new System.IO.DirectoryInfo(dirPath);
                }
                return _directory;
            }
        }
        #endregion

        #region analyzer
        private Analyzer _analyzer = null;
        /// <summary>
        /// 分析器
        /// </summary>
        public Analyzer Analyzer
        {
            get
            {
                if (_analyzer == null)
                {
                    _analyzer = new KiraNet.GutsMvc.BBS.Infrastructure.JiebaAnalyzer();//jieba分词分析器
                }
                else if (_analyzer is Lucene.Net.Analysis.Standard.StandardAnalyzer)
                {
                    _analyzer.Close();
                    _analyzer = new KiraNet.GutsMvc.BBS.Infrastructure.JiebaAnalyzer();
                }

                return _analyzer;
            }
        }
        #endregion

        #region 根据特定的中英文语句选择分析器
        public Analyzer GetAnalyzer(string keyword)
        {
            if (keyword == null)
            {
                if (_analyzer == null)
                {
                    _analyzer = new KiraNet.GutsMvc.BBS.Infrastructure.JiebaAnalyzer();//jieba分词分析器
                }

                return _analyzer;
            }

            Match mInfo = Regex.Match(keyword, @"[\u4e00-\u9fa5]"); // 判断中英文
            if (mInfo.Success) //成功
            {
                if (_analyzer == null)
                    _analyzer = new KiraNet.GutsMvc.BBS.Infrastructure.JiebaAnalyzer();//jieba分词分析器
                else if (!(_analyzer is JiebaAnalyzer))
                {
                    _analyzer.Close();
                    _analyzer = new KiraNet.GutsMvc.BBS.Infrastructure.JiebaAnalyzer();
                }
            }
            else
            {
                if (_analyzer == null)
                    _analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Version);// 标准分析器
                else if (!(_analyzer is Lucene.Net.Analysis.Standard.StandardAnalyzer))
                {
                    _analyzer.Close();
                    _analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Version);
                }
            }

            return _analyzer;
        }
        #endregion

        #region version
        private static Lucene.Net.Util.Version _version = Lucene.Net.Util.Version.LUCENE_30;
        /// <summary>
        /// 版本号枚举类
        /// </summary>
        public Lucene.Net.Util.Version Version
        {
            get
            {
                return _version;
            }
        }
        #endregion
    }
}