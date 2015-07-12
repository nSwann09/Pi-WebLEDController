# Pi-WebLEDController
### Description

This project enables control of leds connected to a Rasberry Pi via a web interface, no GUI on he Pi is needed.

### The Hardware

You will need

*   A Rasberry Pi 2 - (connected to the same net work as your browser)
*   A Breadboard k
*   a number of LEDs
*   An equal number of 220 ohm Resitors
*   Wires

You can connect upto 17 LEDS to the GPIO pins on the PI 2 (Circled Green)

![](http://www.raspberrypi-spy.co.uk/wp-content/uploads/2012/06/Raspberry-Pi-GPIO-Layout-Model-B-Plus-rotated-2700x900.png)

#### Fritzing Diagram:

[![Fritzing Web LEDController](http://s1.postimg.org/3trgnq9u3/Fritzing_Web_LEDController.jpg)](http://postimg.org/image/3trgnq9u3/)

#### Schematic Diagram:

[![Fritzing Web LEDController Schematic](http://s15.postimg.org/rdi6e7ji3/Fritzing_Web_LEDController_Schematic.png)](http://postimage.org/)  

### Directions:

1.  Setup the Raspberry Pi as in the above diagrams (any of the GPIO pins can be used)
2.  Browse to Http://{{RasberryPI_IP}}:8000/index.html and use the Api information to:

    Add an LED via the GPIO number.

    Then control its state.

    And Remove the LED(s) when you are done.
