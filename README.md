# SimpleEngine
 2D Game Engine for WIP

# Things Engine must do
 ## Handle user input
   - mouse move / click
   - keypress / release / hold

 ## Draw on screen
   - get frame from spritesheet
   - draw to correct position (centered or not)
   - animate along frames
   - layer objects correctly

 ## Handle boundaries
   - hitboxes are stopped by boundaries
   - player's hitbox can interact with other hitboxes
     - pushing other objects around, passing through objects

 ## Display text on screen
   - draw textbox
   - read from script file
   - contain multiple routes (variable-dependent)
   - display multiple-choice question interfaces

 ### Multiple-Choice question interfaces
   - display with text
   - resize per amount of answers
   - direct next line of text (paths in script file)
   - respond to player input
   - store answer for later use

  ## Handle save-game files
   - write and interpret Brainfuck code
   - store save-specific game states
   - read from Brainfuck and restore state
     - place in script
     - items in inventory
     - health
     - stats
     - effects
     - current scene
     - player position
