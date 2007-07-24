function CheckAnswer()
{
    var firstInt = $get('firstNumber').innerText;
    var secondInt = $get('secondNumber').innerText;
    var userAnswer = $get('userAnswer');
    
    if ((Number.parseLocale(firstInt) + Number.parseLocale(secondInt)) == userAnswer.value)
    {
        alert(Answer.Correct);
        return true;
    }
    else
    {
        alert(Answer.Incorrect);
        return false;
    }
}
