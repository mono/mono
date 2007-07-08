Type.registerNamespace("ScriptLibrary");
ScriptLibrary.BorderAnimation = function(startColor, endColor, duration) {
    this._startColor = startColor;
    this._endColor = endColor;
    this._duration = duration;
}
ScriptLibrary.BorderAnimation.prototype = {
    animatePanel: function(panelElement) {
        var s = panelElement.style;
        var startColor = this._startColor;
        var endColor = this._endColor;
        s.borderColor = startColor;
        window.setTimeout(
            function() {{ s.borderColor = endColor; }},
            this._duration
        );
    }
}
ScriptLibrary.BorderAnimation.registerClass('ScriptLibrary.BorderAnimation', null);

var panelUpdatedAnimation = new ScriptLibrary.BorderAnimation('blue', 'gray', 1000);
var postbackElement;

Sys.Application.add_load(ApplicationLoadHandler);
function ApplicationLoadHandler() {
    Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(beginRequest);
    Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);
}

function beginRequest(sender, args) {
    postbackElement = args.get_postBackElement();
}
function pageLoaded(sender, args) {
    var updatedPanels = args.get_panelsUpdated();
    if (typeof(postbackElement) === "undefined") {
        return;
    } 
    else if (postbackElement.id.toLowerCase().indexOf('animate') > -1) {
        for (i=0; i < updatedPanels.length; i++) {            
            panelUpdatedAnimation.animatePanel(updatedPanels[i]);
        }
    }

}
if(typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();

