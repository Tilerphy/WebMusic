var SystemContext =
              {
                  musicType: "System", //Uploaded
                  directPlayClicked: false,
                  //0 : random
                  //1 : single-music-loop
                  loopType: 0,
                  timerJobs: [],
                  isListDialogOpen: false,
                  selectedList: "listenedCount",
                  listPage: 0,
                  currentVolume: 0.5
              };
var LoopType =
    {
        Random: 0,
        SingleMusicLoop :1
    };
var MusicContext =
        {
            currentPosition: 0.0,
            currentMusic: "",
            currentMusicId: "",
            currentMusicFileName: "",
            currentMusicTag: "",
            currentMusicPic:""
        };

function shareMusic()
{
    $.get("/api/Music/ShareMusic?musicId=" + MusicContext.currentMusicId).complete(function (result)
    {
        if (result.readyState == 4)
        {
            $("#sharedUrl").val(JSON.parse(result.responseText));
            $("#sharedUrl").show();
            
        }
    });
}

function musicList(element)
{
    
    if (SystemContext.selectedList != element.target.id) {
        SystemContext.listPage = 0;
    }
    $("#pageSize").val(SystemContext.listPage + 1);
    SystemContext.selectedList = element.target.id;
    $.get("/api/Music/ListMusics?sortMethod=" + element.target.id + "&page=" + SystemContext.listPage).complete(function (result)
    {
        if (result.readyState == 4)
        {
            var musics = JSON.parse(result.responseText);
            var htmlStr = "";
            if (musics.length < 10) {
                $("#nextPage").button({ disabled: true });
            } else
            {
                $("#nextPage").button({ disabled: false });
            }
            if (SystemContext.listPage == 0) {
                $("#prevPage").button({ disabled: true });
            }
            for (var i in musics)
            {
                var musicName = musics[i].TagInfo == null || musics[i].TagInfo == "-" ? decodeURIComponent(musics[i].MusicFileName) : musics[i].TagInfo;
                htmlStr += "<div class='MusicListItem'><a  class='MusicListItemLink' onclick='return musicListItemLinkClicked(event)' id='" + musics[i].MusicID + "' href='#'>" + musicName + "</a></div>";
                
            }
            $("#musicNameList").html(htmlStr);

            
        }
    });
}

function musicListItemLinkClicked(element)
{

        selectedMusic(element.target.id, function ()
        {
            $("#musicList").dialog("close");
            SystemContext.isListDialogOpen = false;
        });
        return false;

}
    function initialUI() {
        $("#DownloadButton").button().click(function () {
            open(MusicContext.currentMusic, "__blank");
        });
        $("#sharedUrl").click(function () { $(this).select() });
        $("#shareButton").button().click(shareMusic);
        $("#prevPage").button().click(function ()
        {

            var virElement = {
                target:
                    {
                        id: SystemContext.selectedList
                    }
            };
            SystemContext.listPage--;
            musicList(virElement);

            if (SystemContext.listPage == 0) {
                $("#prevPage").button({ disabled: true });
            }
            
        });

        $("#nextPage").button().click(function () {

            var virElement = {
                target:
                    {
                        id: SystemContext.selectedList
                    }
            };
            SystemContext.listPage++;
            musicList(virElement);
            if (SystemContext.listPage > 0) {
                $("#prevPage").button({ disabled: false });
            }
        });
        $("#listButton").button().click(function () {
            if (SystemContext.isListDialogOpen == false) {
                $("#selections").buttonset();
                $("#musicList").dialog({ width: 800, height: 430 });
                $("#listenedCount").button().click(musicList);
                $("#createdTime").button().click(musicList);
                $("#sharedListenedCount").button().click(musicList);

                var virElement =
                    {
                        target:
                            {
                                id: SystemContext.selectedList
                            }
                    };
                musicList(virElement);
                SystemContext.isListDialogOpen = true;
            }
            else
            {
                SystemContext.isListDialogOpen = false;
                $("#musicList").dialog("close");
            }

        });
        $("#neverPlayButton").button().click(onNeverPlayClicked);
        $(".MusicUI .InfoAndControl .Control .SeekPrevControl").button
            (
                {
                    text: false,
                    icons:
                        {
                            primary: "ui-icon-seek-prev"
                        }
                }
            ).click(onSeekPrevControlClick);
        $(".MusicUI .InfoAndControl .Control .PlayControl").button
            (
                {
                    text: false,
                    icons:
                        {
                            primary: "ui-icon-play"
                        }
                }
            ).click(onPlayClicked);
        $(".MusicUI .InfoAndControl .Control .NextControl").button
            (
                {
                    text: false,
                    icons:
                        {
                            primary: "ui-icon-seek-end"
                        }
                }
            ).click(onNextControlClicked);
        $(".MusicUI .InfoAndControl .Control .SeekNextControl").button
            (
                {
                    text: false,
                    icons:
                        {
                            primary: "ui-icon-seek-next"
                        }
                }
            ).click(onSeekNextControlClick);

        $(".MusicUI .InfoAndControl .Control .Voice").slider(
            {
                max: 100,
                value: SystemContext.currentVolume*100,
                slide: setVolume,
                change: setVolume
            });
        $("#loadingProgress").progressbar({ value: false });

    }

    function setVolume()
    {
        SystemContext.currentVolume = $(".MusicUI .InfoAndControl .Control .Voice").slider("value") / 100;
        musicControl.volume = SystemContext.currentVolume;
    }

    function loading(isloading) {
        if (isloading) {
            $("#loadingDialog").dialog({ modal: true, resizable: false, title: null, height: 60 });
        } else {
            $("#loadingDialog").dialog("close");
        }
    }

    function onNextControlClicked() {
        switch (SystemContext.loopType)
        {
            case 0:
                randomMusic();
                break;
            case 1:
                selectedMusic(MusicContext.currentMusicId);
                break;
            default:
                randomMusic();
        }

    }

    function onSeekNextControlClick() {

        MusicContext.currentPosition += 10;
        if (MusicContext.currentPosition >= musicControl.duration) {
            onNextControlClicked();
        } else {
            onPlayClicked();
        }
    }

    function onSeekPrevControlClick() {
        MusicContext.currentPosition -= 10;
        if (MusicContext.currentPosition < 0) {
            MusicContext.currentPosition = 0;
        }
        onPlayClicked();
    }

    function onPlayClicked(isVirtual) {
        if (!isVirtual) {
            musicControl.currentTime = MusicContext.currentPosition;
        }
        musicControl.play();

        $(".MusicUI .InfoAndControl .Control .PlayControl").button
            (
                {
                    text: false,
                    icons:
                        {
                            primary: "ui-icon-pause"
                        }
                }
            ).unbind("click").click(onPauseClicked);
    }


    function onMusicControlPaused() {
        MusicContext.currentPosition = musicControl.currentTime;
    }

    function onNeverPlayClicked() {
        $.get("/api/Music/SetNeverPlay?musicId=" + MusicContext.currentMusicId).complete(function (result) {
            if (result.readyState == 4) {
                if (parseInt(result.responseText) > 0) {
                    randomMusic();
                }
            }
        });
    }


    function onPauseClicked() {
        musicControl.pause();
        $(".MusicUI .InfoAndControl .Control .PlayControl").unbind("click");
        $(".MusicUI .InfoAndControl .Control .PlayControl").button
            (
                {
                    text: false,
                    icons:
                        {
                            primary: "ui-icon-play"
                        }
                }
            ).unbind("click").click(onPlayClicked);
    }

    function initialControlEvent() {
        musicControl.onpause = onMusicControlPaused;
        musicControl.addEventListener("ended", onNextControlClicked);
        musicControl.addEventListener("timeupdate", onTimeUpdating);
        musicControl.autoPlay = function () {
            onPlayClicked(true);
        }

        musicControl.addEventListener("error", onMusicControlPlayInError);


    }

    function onMusicControlPlayInError() {
        //record
        //db.save();
        onNextControlClicked();
    }

    function onTimeUpdating() {
        var lastTime = musicControl.duration - musicControl.currentTime;
        $(".MusicUI .InfoAndControl .MusicName .MusicProgressbar").progressbar({
            value: (musicControl.currentTime / musicControl.duration) * 100,
        });
        var lastMinutes = parseInt(lastTime / 60);
        var lastMinutesStr = "";
        if (lastMinutes < 10) {
            lastMinutesStr = "0" + lastMinutes;
        } else {
            lastMinutesStr = lastMinutes;
        }
        var lastSecs = parseInt(lastTime % 60);
        var lastSecsStr = "";
        if (lastSecs < 10) {
            lastSecsStr = "0" + lastSecs;
        } else {
            lastSecsStr = lastSecs;
        }
        var lastTimeStr = (lastMinutesStr ? lastMinutesStr : "00") + ":" + (lastSecsStr ? lastSecsStr : "00");
        $(".MusicUI .InfoAndControl .MusicName .Time .TimeContent").text(lastTimeStr);

    }

    function randomMusic(callback) {
        loading(true);
        $("#sharedUrl").hide();
        $.get("/api/Music/GetRandomMusic").complete(function (result) {
            if (result.readyState == 4) {

                var selected = JSON.parse(result.responseText);
                bindDataToContext(selected);
                bindContextToControl();
                $("#loadingDialog").dialog("close");
                if (callback) {
                    callback(selected);
                }
            } else {
                if (callback) {
                    callback(null);
                }
            };
        });
    }

    function selectedMusic(musicId, callback) {
        loading(true);
        $("#sharedUrl").hide();
        $.get("/api/Music/GetMusic?id="+musicId).complete(function (result) {
            if (result.readyState == 4) {

                var selected = JSON.parse(result.responseText);
                bindDataToContext(selected);
                bindContextToControl();
                $("#loadingDialog").dialog("close");
                if (callback) { callback(selected); }
            } else {
                if (callback) {
                    callback(null);
                }
            };
        });
    }

    

    function bindDataToContext(music) {
        MusicContext.currentPosition = 0;
        MusicContext.currentMusic = music.MusicFilePath;
        MusicContext.currentMusicId = music.MusicID;
        MusicContext.currentMusicFileName = decodeURIComponent(music.MusicFileName);
        MusicContext.currentMusicTag = music.TagInfo == null || music.TagInfo == "-" ? MusicContext.currentMusicFileName : music.TagInfo;
        MusicContext.currentMusicPic = music.TagPicFileName == null || music.TagPicFileName == "" ? "/Content/themes/base/images/nullimg.png" : (MusicContext.currentMusic + ".png");
    }

    function bindContextToControl() {
        $(".MusicUI .InfoAndControl .MusicName .MusicNameContent").text(MusicContext.currentMusicTag);
        $("#backgroupIcon").attr("src", MusicContext.currentMusicPic);
        musicControl.src = MusicContext.currentMusic;
        musicControl.autoPlay();
    }

    function randomMusicCallback() {

    }


    var Img = function () {
        var T$ = function (id) { return document.getElementById(id); }
        var ua = navigator.userAgent,
            isIE = /msie/i.test(ua) && !window.opera;
        var i = 0, sinDeg = 0, cosDeg = 0, timer = null;
        var startR = function (target) {
            target = T$(target);
            var orginW = target.clientWidth, orginH = target.clientHeight;
            clearInterval(timer);
            function run(angle) {
                if (isIE) { // IE
                    ////target.style.webkitTransform = 'rotate(' + angle + 'deg)';
                    //cosDeg = Math.cos(angle * Math.PI / 180);
                    //sinDeg = Math.sin(angle * Math.PI / 180);
                    //with (target.filters.item(0)) {
                    //    M11 = M22 = cosDeg; M12 = -(M21 = sinDeg);
                    //}
                    //target.style.top = (orginH - target.offsetHeight) / 2 + 'px';
                    //target.style.left = (orginW - target.offsetWidth) / 2 + 'px';
                    //var sinDeg = Math.sin(angle / 180 * Math.PI);
                    //var cosDeg = Math.cos(angle / 180 * Math.PI);
                    //target.style.filter = "progid:DXImageTransform.Microsoft.Matrix(" +
                    //        "M11=" + cosDeg + ",M12=" + (-sinDeg) + ",M21=" + sinDeg + ",M22=" + cosDeg + ",SizingMethod='auto expand')";
                    ////target.style.top = (orginH - target.offsetHeight) / 2 + 'px';
                    ////target.style.left = (orginW - target.offsetWidth) / 2 + 'px';
                } else if (target.style.MozTransform !== undefined) {  // Mozilla
                    target.style.MozTransform = 'rotate(' + angle + 'deg)';
                } else if (target.style.OTransform !== undefined) {   // Opera
                    target.style.OTransform = 'rotate(' + angle + 'deg)';
                } else if (target.style.webkitTransform !== undefined) { // Chrome Safari
                    target.style.webkitTransform = 'rotate(' + angle + 'deg)';
                } else {
                    target.style.transform = "rotate(" + angle + "deg)";
                }
            }

            timer = setInterval(function () {
                i += 1;
                run(i);
            }, 10);
        }

        var stopR = function () {
            clearInterval(timer);
        }


        return { start: startR, stop: stopR };
    }();

    function initialBase(callback) {
        loading(true);
        $.get("/api/SaltApi/GetCurrentSalt").complete(function (result) {

            if (result.readyState == 4) {
                SystemContext.salt = JSON.parse(result.responseText);
            }

            callback();
        });
    }



