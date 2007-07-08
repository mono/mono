<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" 
    "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Custom Events Example</title>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" >
        <Scripts>
           <asp:ScriptReference Path="question.js" />
           <asp:ScriptReference Path="section.js" />
       </Scripts>
    </asp:ScriptManager>
    <script type="text/javascript">
    // Add handler to init event
    Sys.Application.add_init(appInitHandler);

    function appInitHandler() {
      // create components
      $create(Demo.Question, {correct: '3'},
        {select: onAnswer},null, $get('Question1'));
      $create(Demo.Question, {correct: '3'},
        {select: onAnswer},null, $get('Question2'));
      $create(Demo.Question, {correct: '3'},
        {select: onAnswer},null, $get('Question3'));
      $create(Demo.Question, {correct: '3'},
        {select: onAnswer},null, $get('Question4'));
      $create(Demo.Section, null,
        {complete: onSectionComplete},null, $get('group1'));
      $create(Demo.Section, null,
        {complete: onSectionComplete},null, $get('group2'));
    }

    function onAnswer(question) {
        // If all questions in this section answered, 
        // raise complete event
        var section = question.get_element().parentElement;
        var questions = section.children;
        $get(question.get_id() + 'Status').innerHTML = '';
        for (var i=0; i<questions.length; i++) {
            if (questions[i].selectedIndex === -1) {
                return;
            }
        }
        $find(section.id).raiseComplete();
    }

    function onSectionComplete(section) {
        // Change background color of <div>.
        section.get_element().style.backgroundColor = 'yellow';
    }

    function done() {
        // Display correct answers where needed.
        var c = Sys.Application.getComponents();
        for (var i=0; i<c.length; i++) {
            var type = Object.getType(c[i]).getName();
            if (type !== 'Demo.Question') continue;
            var element = c[i].get_element();
            var answer = element.selectedIndex;
            var correct = $find(c[i].get_id()).get_correct();
            var statusElement = c[i].get_id() + 'Status';
            if (answer !== correct) {
                $get(statusElement).innerHTML = 'Incorrect. Try again.';
            }
            else
            {
                $get(statusElement).innerHTML = 'Correct.';
            }
        }
    }

    function resethandler() {
        var c = Sys.Application.getComponents();
        for (var i=0; i<c.length; i++) {
            var type = Object.getType(c[i]).getName();
            if (type === 'Demo.Question') {
                var element = c[i].get_element();
                element.selectedIndex = -1;
                var answer = element.selectedIndex;
                var statusElement = c[i].get_id() + 'Status';
                $get(statusElement).innerHTML = '';
            }
            else if (type === 'Demo.Section') {
                c[i].get_element().style.backgroundColor = 'White';
                
            }
        }
    }
    </script>
    <h3>Addition</h3><br />
    <div id="Group1">
        2 + 2 = 
        <select id="Question1">
            <option>2</option>
            <option>22</option>
            <option>4</option>
            <option>5</option>
        </select><span id="Question1Status"></span><br />
        2 + 3 = 
        <select id="Question2" >
            <option>3</option>
            <option>23</option>
            <option>5</option>
            <option>6</option>
        </select><span id="Question2Status"></span><br />
    </div><br /> <br />   
    <h3>Subtraction</h3><br />
    <div id="Group2">
        2 - 1 = 
        <select id="Question3" >
            <option>2</option>
            <option>0</option>
            <option>1</option>
            <option>-2</option>
        </select><span id="Question3Status"></span><br />
        2 - 2 = 
        <select id="Question4" >
            <option>2</option>
            <option>-2</option>
            <option>0</option>
            <option>-4</option>
        </select><span id="Question4Status"></span><br />
    </div><br /><br />
    <input id="Submit1" type="button" value="Check Answers" onclick="done()" />
    <input id="Reset1" type="button" value="Start Again" onclick="resethandler()" />
</form>
</body>
</html>
