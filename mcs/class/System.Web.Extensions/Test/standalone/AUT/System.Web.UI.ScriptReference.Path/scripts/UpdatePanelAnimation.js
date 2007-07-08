BorderAnimation = function(color) {
    this._color = color;
}

BorderAnimation.prototype = {
    animate: function(panelElement) {
        var s = panelElement.style;
        s.borderWidth = '2px';
        s.borderColor = this._color;
        s.borderStyle = 'solid';

        window.setTimeout(
            function() {{
                s.borderWidth = 0;
            }},
            500);
    }
}
        
if(typeof(Sys) !== "undefined")Sys.Application.notifyScriptLoaded();
