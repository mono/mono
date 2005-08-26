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

      function test_attrib(elem, attr, out_come_name, name, input) {
	  var out_come = get_elem (out_come_name);
	  if (!elem.getAttribute) {
	    out_come.innerHTML = "IE sucks!";
	    out_come.className = "failed";
	  } else {
	    var input_elem = get_elem (input);
	    var input_value = input_elem.getAttribute ("value");

	    if (elem.getAttribute (attr) != input_value) {
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
	  // If this is the first time, "target" should be empty
	  if (!elem.hasAttribute ("target")) {
	    var out_come = get_elem ("outcome_name");
	    out_come.innerHTML = "Test not run yet";
	    out_come.className = "notrun";

	    out_come = get_elem ("outcome_method");
	    out_come.innerHTML = "Test not run yet";
	    out_come.className = "notrun";

	    out_come = get_elem ("outcome_enctype");
	    out_come.innerHTML = "Test not run yet";
	    out_come.className = "notrun";

	    out_come = get_elem ("outcome_target");
	    out_come.innerHTML = "Test not run yet";
	    out_come.className = "notrun";
	  } else {
	    test_attrib (elem, "name", "outcome_name", "Name", "name");
	    test_attrib (elem, "method", "outcome_method", "Method", "method");
	    test_attrib (elem, "enctype", "outcome_enctype", "Enctype", "enctype");
	    test_attrib (elem, "target", "outcome_target", "Target", "target");
	  }
	}
      }
    </script>

    <script runat="server">
      void submit (object sender, EventArgs e)
      {
        form1.Name = name.Value;
        form1.Method = method.Value;
        form1.Enctype = enctype.Value;
        form1.Target = target.Value;
      }
    </script>

    <style type="text/css" media="screen">
      <!--
	.notrun { background-color: blue; color: white;}
	.passed { background-color: green; color: white;}
	.failed { background-color: red; color: white;}
      -->
    </style>
    <form id="form1" runat="server">
      Name: <input type="text" id="name" runat="server"/> <br>
      Method: <input type="text" id="method" runat="server"/> <br>
      Enctype: <input type="text" id="enctype" runat="server"/> <br>
      Target: <input type="text" id="target" runat="server"/> <br>
      <input type="submit" value="Click Me!" OnServerclick="submit" runat="server" />
    </form>
    <div id="outcome_name" class="">Default text. Should not be seen.</div>
    <div id="outcome_method" class="">Default text. Should not be seen.</div>
    <div id="outcome_enctype" class="">Default text. Should not be seen.</div>
    <div id="outcome_target" class="">Default text. Should not be seen.</div>
  </body>
</html>

