function Notify(sender, arg)
{
    ActivateAlertDiv('visible', 'NotifyDiv');
    setTimeout("ActivateAlertDiv('hidden', 'NotifyDiv')", 1000);
}
function ActivateAlertDiv(visstring, elem)
{
    var adiv = document.getElementById(elem);
    adiv.style.visibility = visstring;
}
