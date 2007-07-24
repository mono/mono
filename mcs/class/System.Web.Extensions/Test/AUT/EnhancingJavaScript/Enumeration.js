Type.registerNamespace("Demo");

// Define an enumeration type and register it.
Demo.Color = function(){};
Demo.Color.prototype = 
{
    Red:    0xFF0000,
    Blue:   0x0000FF,
    Green:  0x00FF00,
    White:  0xFFFFFF 
}
Demo.Color.registerEnum("Demo.Color");

