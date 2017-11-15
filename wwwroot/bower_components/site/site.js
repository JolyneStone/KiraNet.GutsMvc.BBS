
function cateShow(id) {
    var li = $("#" + id);
    if (!li.hasClass("list-group-item-success")) {
        li.addClass("list-group-item-success");
    }
}

function cateHidden(id) {
    var li = $("#" + id);
    if (li.hasClass("list-group-item-success")) {
        li.removeClass("list-group-item-success");
    }
}

function pushBBSli(id, value) {
    return '<li><a class="alink" href= "/bbs/index/' + id + '">' + value + '</a></li>';
}

function pushBBSLiTag(arr, id) {
    if (arr.length > 0) {
        $("#bbslist" + id).html('<li style="display:none;" class="dropdown-header"></li>' + arr.join(''));
    } else {
        $("#bbslist" + id).html('<li class="dropdown- header">暂无</li>')
    }
}

function bindLogBtn(userId, page, tabId, btnId, baseUrl, clickUrl, callback) {
    $.get(baseUrl, { id: userId, page: page }, function (data) {
        if (data != null) {
            if (data.IsOk == false) {
                $("#" + tabId + " tbody").html('<tr><td>获取失败，稍后重试</td></tr>');
                $("#" + tabId + " tfoot").html('');
                return;
            }

            var btnLeftId = btnId + 'Left';
            var btnRightId = btnId + 'Right';

            var trArr = [];
            $.each(data.Data.PageData, function (i, item) {
                trArr.push('<tr><td><a href="' + clickUrl + '?id=' + item.Id + '">' + item.Message + '</a></td>' +
                    '<td><a href="' + clickUrl + '?id=' + item.Id + '">' + item.CreateTime + '</a></td>' +
                    '</tr>');
            });
            if (trArr.length > 0) {
                $("#" + tabId + " tbody").html(trArr.join(''));
                var htmlStr = '<tr><td style="text-align: center; padding-top: 2px; padding-bottom: 2px;" colspan="2">';
                var hasLeft = false;
                var hasRight = false;
                if (data.Data.PreviousPage > 0) {
                    hasLeft = true;
                    htmlStr = htmlStr + '<span id="' + btnLeftId + '" class="fa fa-fw fa-chevron-left pointer"></span>';
                }
                if (data.Data.NextPage > 0) {
                    hasRight = true;
                    htmlStr = htmlStr + '<span id="' + btnRightId + '" class="fa fa-fw fa-chevron-right pointer"></span>';
                }

                htmlStr = htmlStr + '</td></tr>';
                $("#" + tabId + " tfoot").html(htmlStr);
                if (typeof callback != 'function') {
                    return;
                }
                if (hasLeft) {
                    $("#" + btnLeftId).unbind('click');
                    $("#" + btnLeftId).on('click', function () {
                        callback(tabId, baseUrl, userId, data.Data.PreviousPage);
                    });
                }
                if (hasRight) {
                    $("#" + btnRightId).unbind('click');
                    $("#" + btnRightId).on('click', function () {
                        callback(tabId, baseUrl, userId, data.Data.NextPage);
                    });
                }
            } else {
                $("#" + tabId + " tbody").html('<tr><td>暂无</td></tr>');
                $("#" + tabId + " tfoot").html('');
            }
        }
    });
}

var gutsmvc = function () {
    var now = new Date();

    return {
        // 绑定搜索记录
        bindSearchHistory: function (tabName) {
            var his = this.getSearchHistory();
            var arr = [];
            $.each(his, function (i, item) {
                arr.push('<option value="' + item + '">');
            });

            $('datalist[name=' + tabName + ']').html(arr.join(''));

        },
        // 获取搜索记录
        getSearchHistory: function () {
            var storage = window.localStorage.getItem("gutsmvc_searchhistory");
            if (storage != null && storage != undefined) {
                return JSON.parse(storage);
            } else {
                return new Array();
            }
        },
        // 压入新的搜索记录并移除老记录，搜索记录只保存最近5项
        pushSearchHistory: function (value) {
            var his = this.getSearchHistory();
            his.push(value);
            if (his.length > 5) {
                his.shift();
            }

            window.localStorage.setItem("gutsmvc_searchhistory", JSON.stringify(his));
        },
        /// 获取论坛列表
        getBBSList: function () {
            $.get("/home/getbbslist", function (data) {
                // 用笨方法，懒得去研究js的写法了
                var bbsArr1 = [];
                var bbsArr2 = [];
                var bbsArr3 = [];
                var bbsArr4 = [];
                var bbsArr5 = [];
                var bbsArr6 = [];
                var bbsArr7 = [];
                var bbsArr8 = [];
                var bbsArr9 = [];
                if (data) {
                    if (data.IsOk && data.Data != null) {
                        var bbslist = data.Data;
                        $.each(bbslist, function (i, item) {
                            switch (item.BBSType) {
                                case '1':
                                    bbsArr1.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '2':
                                    bbsArr2.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '3':
                                    bbsArr3.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '4':
                                    bbsArr4.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '5':
                                    bbsArr5.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '6':
                                    bbsArr6.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '7':
                                    bbsArr7.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '8':
                                    bbsArr8.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                case '9':
                                    bbsArr9.push(pushBBSli(item.BBSId, item.BBSName));
                                    break;
                                default: break;
                            }
                        });
                    }
                }

                pushBBSLiTag(bbsArr1, '1');
                pushBBSLiTag(bbsArr2, '2');
                pushBBSLiTag(bbsArr3, '3');
                pushBBSLiTag(bbsArr4, '4');
                pushBBSLiTag(bbsArr5, '5');
                pushBBSLiTag(bbsArr6, '6');
                pushBBSLiTag(bbsArr7, '7');
                pushBBSLiTag(bbsArr8, '8');
                pushBBSLiTag(bbsArr9, '9');
            });
        },
        /// 获取推荐
        getRecommends: function (divId, countId) {
            $.get("/home/getrecommends", function (data) {
                var recommendArr = [];
                if (data) {
                    if (data.IsOk && data.Data != null) {
                        var recommends = data.Data;
                        $.each(recommends, function (i, item) {
                            recommendArr.push('<div class="col-md-12 caption" title="' + item.TopicName + '">' +
                                '<a href="/bbs/topic/' + item.TopicId + '" target="_blank">' +
                                '<img src="' + item.UserPhoto + '" title="' + item.UserName + '" style="width: 30%;" />' +
                                '<span class="text-muted" style="margin-left:3px;">' + item.TopicName + '</span>' +
                                '</a>' +
                                '</div>');
                        });
                    }
                }

                if (recommendArr.length > 0) {
                    $("#" + countId).html(recommendArr.length);
                    $("#" + divId).html(recommendArr.join(''));
                } else {
                    $("#" + divId).html("<h4>暂无</h4>");
                    $("#" + countId).html('0');
                }
            });
        },
        /// 获取用户登录信息
        getUser: function () {
            setTimeout(function () {
                $.get("/member/getlogininfo", function (data) {
                    var loginArr = [];
                    if (data) {
                        if (data.IsOk && data.Data != null) {
                            var user = data.Data;
                            loginArr.push('<li class="loginli"><a href="/usercenter/index" style="padding-top:10px;"><img id="userheadphoto" class="img img-circle" src="' + user.HeadPhoto + '" style="width:30px;height:30px;" /></a></li>');
                            loginArr.push('<li class="loginli"><a href="/usercenter/index"><span id="usernickname" style="color:red">' + user.UserName + '</span></a></li>');
                            loginArr.push('<li class="loginli"><a href="/chat/index">私信</a></li>');
                            loginArr.push('<li class="loginli"><a href="/usercenter/index">Personal Center</a></li>');
                            loginArr.push('<li class="loginli"><a href="/member/loginout">Sign out</a></li>');
                        } else {
                            console.log(data.Msg);
                        }
                    }
                    if (loginArr.length > 0) {
                        $("#ul_login").html(loginArr.join(''));
                    }
                });
            }, 1000);
        },
        /// 适用于本网站中所有表单的提交
        bindSubmitBtn: function () {
            $("input[name='btnSubmit']").on("click", function () {
                // 为了使js更简单，我们将本网站上的所有表单、提醒、提交按钮都设置为统一的命名
                // 首先：我们要隐藏提交按钮，显示提醒，为了更良好的交互
                var _btn = $(this);
                _btn.addClass("hide");
                var _msg = $("#msgbox");
                _msg.html("提交中，请稍后...");

                var _form = $("form[name='form_submit']");

                if (_form != null) {
                    _form.submit();
                } else {
                    _btn.removeClass("hide");
                    _msg.html("");
                }
            });
        },
        /// 获取记录条数
        getLogCount: function (tabId, userId, url) {
            if (tabId.length <= 0 && userId == null && url == null && url.length == 0) {
                return;
            }

            $.get(url, { userId: userId }, function (data) {
                if (data != null && data.IsOk && data.Data != null) {
                    $('#' + tabId).html(data.Data);
                }
            });
        },
        /// 获取用户发帖记录
        getUserTopicLog: function (tabId, userId, currentPage) {
            if (tabId.length <= 0 && userId == null) {
                return;
            }

            get(tabId, '/usercenter/getusertopiclog/', userId, currentPage);

            function get(tableId, url, id, page) {
                bindLogBtn(id, page, tableId, 'userTopic', url, "/bbs/topic/", get);
            };
        },
        /// 获取回帖记录
        getUserReplyLog: function (tabId, userId, currentPage) {
            if (tabId.length <= 0 && userId == null) {
                return;
            }

            get(tabId, '/usercenter/getuserreplylog/', userId, currentPage);

            function get(tableId, url, id, page) {
                bindLogBtn(id, page, tableId, 'replyTopic', url, "/bbs/topic/", get);
            };
        },
        /// 获取用户被关注列表记录
        getByUserStarLog: function (tabId, userId, currentPage) {
            if (tabId.length <= 0 && userId == null) {
                return;
            }

            get(tabId, '/usercenter/getbyuserstarlog/', userId, currentPage);

            function get(tableId, url, id, page) {
                bindLogBtn(id, page, tableId, 'byUserStar', url, "/user/index/", get);
            };
        },
        /// 获取用户关注列表记录
        getUserStarLog: function (tabId, userId, currentPage) {
            if (tabId.length <= 0 && userId == null) {
                return;
            }

            get(tabId, '/usercenter/getuserstarlog/', userId, currentPage);

            function get(tableId, url, id, page) {
                bindLogBtn(id, page, tableId, 'userStar', url, "/user/index/", get);
            };
        },
        /// 生成分页链接
        initPagerOption: function (currentPage, total, pageSize, divId, aName, routeUrl, joinCode) {
            if (total <= 0) {
                return;
            }
            if (currentPage <= 0) {
                currentPage = 1;
            }
            if (pageSize <= 0) {
                pageSize = 15;
            }

            // 计算总页数
            var totalPage = total / pageSize + (total % pageSize > 0 ? 1 : 0);
            if (totalPage <= 0) {
                return;
            }
            else if (totalPage <= currentPage) {
                currentPage = totalPage;
            }

            var url = routeUrl + joinCode;
            // 首页页码
            var htmlCode = '<ul class="pagination"';
            htmlCode = htmlCode + pageProvider(0, url, '首页', aName, false);

            // 上一页
            var previousPage = currentPage - 1;
            if (previousPage > 1) {
                htmlCode = htmlCode + pageProvider(previousPage, url, '上一页', aName, false);
            }


            var leftPage = currentPage - 3;
            if (leftPage <= 0) {
                leftPage = 1;
            }
            var rightPage = currentPage + 3;
            if (rightPage >= totalPage) {
                rightPage = totalPage;
            }

            var i = leftPage;
            for (i; i < rightPage; i++) {
                htmlCode = htmlCode + pageProvider(i, url, String(i), aName, i == currentPage);
            }

            // 下一页
            var nextPage = currentPage + 1;
            if (nextPage < total) {
                htmlCode = htmlCode + pageProvider(nextPage, url, '下一页', aName, false);
            }

            // 尾页
            htmlCode = htmlCode + pageProvider(totalPage - 1, url, '尾页', aName, false) + "</ul>";

            $("#" + divId).html(htmlCode);

            function pageProvider(page, getUrl, pageStr, aStr, isCurrentPage) {

                var code = '<li><a name="' + aStr + '" href="#" data-myhref="' + getUrl + page + '"' + isCurrentPage ? 'class="active"' : '' + '</>'
                '<span>' + pageStr + '</span>' +
                    '</a></li>';
                return code;
            }
        },
        // 发送信息给客服
        sendServiceMessage: function () {
            //$('.write_list').remove();
            //$('.wenwen_btn img').attr('src', '../../wwwroot/images/jp_btn.png');
            //$('.wenwen_btn').attr('onclick', 'to_write()');
            
            $("#btnSend").click(function () {
                var headPhotoUrl = $('#userheadphoto').attr('src');
                $('.write_list').remove();
                var text = $('.write_box input').val(),
                    str = '<div class="question">';
                str += '<div class="heard_img right"><img class="img img-circle" width="60" height="60" src="' + headPhotoUrl + '"></div>';
                str += '<div class="question_text clear"><p>' + text + '</p><i></i>';
                str += '</div></div>';

                if (text == '') {
                    alert('请输入提问！');
                    $('.write_box input').focus();
                } else {
                    $('.speak_box').append(str);
                    $('.write_box input').val('');
                    $('.write_box input').focus();
                    //autoWidth();

                    getServiceMessage(text);
                }
            });

            // 从客服接受信息
            function getServiceMessage(message) {
                $.post('/home/sendquestion', { message: message }, function (data) {
                    var ans = '<div class="answer"><div class="heard_img left"><img width="60" height="60" src="../../wwwroot/images/gutsmvc.png"></div>';
                    //ans += '<div class="answer_text"><p>' + text + '</p><i></i>';

                    if (data && data.IsOk) {
                        //var response = JSON.parse(data.Data);
                        var response = data.Data;
                        switch (Number(response.code)) {
                            case 100000: {
                                // 文本类
                                ans += '<div class="answer_text"><p>' + response.text + '</p><i></i>';
                                break;
                            }
                            case 200000: {
                                // 链接类
                                ans += '<div class="answer_text"><p>' + response.text + '<a href="' + response.url + '">click here</a></p><i></i>';
                                break;
                            }
                            case 302000: {
                                // 新闻类
                                ans += '<div class="answer_text"><p>' + response.text + '</p><i></i>';
                                ans += '</div></div>';

                                if (response.list != null && response.list.length > 0) {
                                    $.each(response.list, function (i, item) {
                                        ans += '</div></div>';

                                        var temp_ans = '<div class="answer"><div class="heard_img left"><img width="60" src="../../wwwroot/images/gutsmvc.png"></div>';
                                        temp_ans += '<div class="answer_text"><p><a href="' + item.detailurl + '">';
                                        if (item.icon != null && item.icon != "") {
                                            temp_ans += '<img src="' + item.icon + '" alt="' + item.source + '" title="' + item.article + '" />';
                                        } else {
                                            temp_ans += item.article;
                                        }

                                        temp_ans += '</a></p><i></i>';

                                        ans += temp_ans;
                                    });
                                }

                                break;
                            }
                            case 308000: {
                                // 菜谱类
                                ans += '<div class="answer_text"><p>' + response.text + '</p><i></i>';
                                ans += '</div></div>';

                                if (response.list != null && response.list.length > 0) {
                                    $.each(response.list, function (i, item) {
                                        ans += '</div></div>';

                                        var temp_ans = '<div class="answer"><div class="heard_img left"><img width="60" src="../../wwwroot/images/gutsmvc.png"></div>';
                                        temp_ans += '<div class="answer_text"><p>' + item.name + ':<a href="' + item.detailurl + '">';
                                        if (item.icon != null && item.icon != "") {
                                            temp_ans += '<img src="' + item.icon + '" alt="' + item.name + '" title="' + item.info + '" />';
                                        } else {
                                            temp_ans += item.info;
                                        }

                                        temp_ans += '</a></p><i></i>';

                                        ans += temp_ans;
                                    });
                                }

                                break;
                            }
                            default: {
                                ans += '<div class="answer_text"><p>' + response.text + '</p><i></i>';
                                break;
                            }
                        }
                    }
                    else {
                        ans += '<div class="answer_text"><p>请检查您的网络</p><i></i>';
                    }

                    ans += '</div></div>';
                    $('.speak_box').append(ans);
                });
            }
        }
    }
}


//初始化
var bbs = new gutsmvc();
//用户信息
bbs.getUser();
//提交按钮
bbs.bindSubmitBtn();
//获取子论坛列表
bbs.getBBSList();