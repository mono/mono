function toggle (name)
{
    var element = document.getElementById (name);
    
    if (element.style.display == 'none')
        element.style.display = '';
    else
        element.style.display = 'none';
}

function highlight (element)
{
    element.style.background = "#eee";
}

function unhighlight (element)
{
    element.style.background = "none";
}

