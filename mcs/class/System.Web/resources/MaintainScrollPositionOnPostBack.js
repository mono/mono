function WebForm_SaveScrollPositionSubmit() {
    this.elements['__SCROLLPOSITIONX'].value = WebForm_GetScrollX();
    this.elements['__SCROLLPOSITIONY'].value = WebForm_GetScrollY();
    if ((typeof(this.oldSubmit) != "undefined") && (this.oldSubmit != null)) {
        return this.oldSubmit();
    }
    return true;
}
function WebForm_SaveScrollPositionOnSubmit() {
    this.elements['__SCROLLPOSITIONX'].value = WebForm_GetScrollX();
    this.elements['__SCROLLPOSITIONY'].value = WebForm_GetScrollY();
    if ((typeof(this.oldOnSubmit) != "undefined") && (this.oldOnSubmit != null)) {
        return this.oldOnSubmit();
    }
    return true;
}
function WebForm_RestoreScrollPosition(currForm) {
	currForm = currForm || theForm;
	var ScrollX = currForm.elements['__SCROLLPOSITIONX'].value;
	var ScrollY = currForm.elements['__SCROLLPOSITIONY'].value;
	if (ScrollX != "" || ScrollY != "")
    	window.scrollTo(ScrollX, ScrollY);
    if ((typeof(this.oldOnLoad) != "undefined") && (this.oldOnLoad != null)) {
        return this.oldOnLoad();
    }
    return true;
}
function WebForm_GetScrollX() {
    if (window.pageXOffset) {
        return window.pageXOffset;
    }
    else if (document.documentElement && document.documentElement.scrollLeft) {
        return document.documentElement.scrollLeft;
    }
    else if (document.body) {
        return document.body.scrollLeft;
    }
    return 0;
}
function WebForm_GetScrollY() {
    if (window.pageYOffset) {
        return window.pageYOffset;
    }
    else if (document.documentElement && document.documentElement.scrollTop) {
        return document.documentElement.scrollTop;
    }
    else if (document.body) {
        return document.body.scrollTop;
    }
    return 0;
}
