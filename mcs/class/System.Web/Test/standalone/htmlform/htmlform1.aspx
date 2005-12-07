<%@ Page Language="C#" %>
<html>
  <head>
    <title>HtmlForm test</title>
  </head>
  <body onload="check_src()">
    <script language="javascript" type="text/javascript">
      function get_elem(id) {
	return (document.getElementById) ? document.getElementById (id) :
		((document.all) ? document.all [id] : null);
      }

      function test_attrib(elem, attr, out_come_name, name, present) {
	  var out_come = get_elem (out_come_name);
	  if (!elem.hasAttribute) {
	    out_come.innerHTML = "IE sucks!";
	    out_come.className = "failed";
	  } else {
	    if (elem.hasAttribute (attr) != present) {
	      out_come.innerHTML = name + " test failed";
	      out_come.className = "failed";
	    } else {
	      out_come.innerHTML = name + " test passed";
	      out_come.className = "passed";
	    }
	  }
      }

      function check_src () {
	var elem = get_elem ("form1");
	if (elem) {
	  // These attributes should always be present
	  test_attrib (elem, "name", "outcome_name", "Name", true);
	  test_attrib (elem, "method", "outcome_method", "Method", true);
	  test_attrib (elem, "action", "outcome_action", "Action", true);
	  test_attrib (elem, "id", "outcome_id", "ID", true);

	  // Test for attributes that should NOT be there
	  test_attrib (elem, "enctype", "outcome_enctype", "Enctype", false);
	  test_attrib (elem, "target", "outcome_target", "Target", false);
	  test_attrib (elem, "wibble", "outcome_wibble", "Wibble", false);
	}
      }
    </script>
    <style type="text/css" media="screen">
      <!--
	.passed { background-color: green; color: white;}
	.failed { background-color: red; color: white;}
      -->
    </style>
    <form id="form1" enctype="" target="" runat="server" />
    <div id="outcome_name" class="">Default text. Should not be seen.</div>
    <div id="outcome_method" class="">Default text. Should not be seen.</div>
    <div id="outcome_action" class="">Default text. Should not be seen.</div>
    <div id="outcome_id" class="">Default text. Should not be seen.</div>
    <div id="outcome_enctype" class="">Default text. Should not be seen.</div>
    <div id="outcome_target" class="">Default text. Should not be seen.</div>
    <div id="outcome_wibble" class="">Default text. Should not be seen.</div>
  </body>
</html>

