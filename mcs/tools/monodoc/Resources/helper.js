function toggle_display (block) {
  var w = document.getElementById (block);
  var t = document.getElementById (block + ":toggle");
  if (w.style.display == "none") {
    w.style.display = "block";
		t.getElementsByTagName("img")[0].setAttribute ("src", "xtree/images/clean/Lminus.gif"); // <img src="xtree/images/clean/Lminus.gif">
  } else {
    w.style.display = "none";
		t.getElementsByTagName("img")[0].setAttribute ("src", "xtree/images/clean/Lplus.gif"); // <img src="xtree/images/clean/Lplus.gif">
  }
}

