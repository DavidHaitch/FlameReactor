var FR = FR || {};
FR.load = function (element) {
    element.load();
    element.muted = true;
    element.play();
};

FR.logRenderStep = function (renderStepEvent) {
    console.log(renderStepEvent);
};

FR.playSound = function () {
    document.getElementById('alertbell').volume = 0.5;
    document.getElementById('alertbell').play();
};

FR.ToggleFullscreen = function () {
    if (!document.fullscreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {
        var elem = document.documentElement;
        if (elem.requestFullscreen) {
            elem.requestFullscreen();
        } else if (elem.webkitRequestFullscreen) { /* Safari */
            elem.webkitRequestFullscreen();
        } else if (elem.msRequestFullscreen) { /* IE11 */
            elem.msRequestFullscreen();
        }
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) { /* Safari */
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) { /* IE11 */
            document.msExitFullscreen();
        }
    }
};

FR.WakeUI = function () {
    var fadeables = document.getElementsByClassName('fadeable');
    for (var i = 0; i < fadeables.length; i++) {
        if (fadeables[i].classList.contains('fadeout')) {
            if (fadeables[i].getAnimations()[0] == undefined || fadeables[i].getAnimations()[0].playState != 'running')
            fadeables[i].classList.remove('fadeout');
        }
        else {
            fadeables[i].classList.add('fadeout');
        }
    }
};