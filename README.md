<h1> MultiPaste </h1>
<ul>
  <li>A free, enhanced clipboard manager for Windows</li>
  <li>Supports text, files, raw images, audio, and even custom formats!</li>
  <li>Remembers what you copied even after closing and reopening MultiPaste</li>
  <li>Support for drag-and-drop on text, files, and image links!</li>
</ul>

<h2> Releases </h2>
The latest release can be found in the <a href="https://github.com/bvancamp99/MultiPaste/releases">releases</a> section of the MultiPaste repository.

<h2> Additional credits </h2>
<a href="https://github.com/Alex78904565">Alex78904565</a>, who has assisted in the early obstacles of this program.

<h3> Working on </h3>
<ul>
  <li>'File->New->Text Item' option</li>
  <li>Ability to pin items to the top.  This would require us to modify the add methods in LocalClipboard to insert at index 0 + numPinnedItems, rather than 0 unconditionally.</li>
  <li>We could remove the window frame and implement our own custom "frame", with X button et al.  This would be a great way to circumvent the problem of setting the frame's color, and also we can have a more custom look to the top.</li>
  <li>Option to edit items.  This feature could be offered via separate menu items in the Other Options dropdown, e.g. "Edit text item" or "Edit file item."  Although it might be better just to have one menu item and/or one button for editing, and the window that pops up is dynamically determined based on the type of ClipboardItem.  This would mean we need to make separate forms for each ClipboardItem type, so at most 5 (could be less if we can conflate, say, forms for the items that use a file system).</li>
  
</ul>

<h3> Fix </h3>
<ul>
  <li>Noticed a trailing image file with no corresponding item in the file/data structures.  It was the snipped fl studio beat raw image thing.</li>
  <li>Moving items is weird when using the search algorithm.  Also, restricting type doesn't mix well with restricting by keyword (which was expected); could fix via new string collection storing the current keys being displayed.</li>
  <li>AudioItem is implemented, but a method of testing it is yet to be found.  Stream.Length might serve as a problem, since its documentation seems to imply that it's only supported in derived classes that **use** Length.</li>
</ul>

<h3> Improve </h3>
<ul>
  <li>Gray out edit option unless selected item is text item</li>
  <li>Might want another string collection in LocalClipboard to store all keys currently displayed on the visual clipboard</li>
  <li>Lots of redundant code in ClipboardItem's children classes; conflate if possible and/or make abstract methods</li>
  <li>Make abstract parent class for ImageItem, AudioItem, and CustomItem.  It would contain a "MyFile" FileInfo property, as well as some operations in the constructor.</li>
  <li>Work on overall color theme implementation, including: changing the border color of a listbox (no native support); Argb implementation; more color theme options.</li>
  <li>Maybe "enter" keypress should give options to edit the item rather than copy it.  It would provide a popup with a textbox.  This edit option should also be available in the context menu (would open popup as normal).</li>
  <li>Clean up the implementation of OnArrowKeyUp and OnArrowKeyDown.  They're kind of a mess again.</li>
  <li>Could potentially conflate IsEquivalent on ImageItem, AudioItem, and CustomItem.  Would need to create file before comparing the ClipboardItems (if possible).  Then we would compare byte length of the files first, then maybe do more checking if that bool returns true (although byte-by-byte checking could become VERY slow).</li>
  <li>Not sure if this would be possible, but making MainWindow static would make a lot of sense.  Then we don't have to pass copies of its instance around to every class.  Issue is then it can't inherit from the Form class (which isn't static).</li>
  <li>Would like to improve ClipboardItem.SetKeyDiff.  It does a lot of things that don't make sense without significant context.  Would also like to move it to LocalClipboard.cs</li>
  <li>Show copied images on the listbox</li>
  <li>Work on MsgLabel and think about desired implementation</li>
  <li>ImageItem.IsEquivalent - with experience, I think it would be better to replace with new image if same size</li>
  <li>CustomItem.IsEquivalent</li>
  <li>Keep menuStrip open when a toggle check item is clicked.</li>
</ul>

<h3> Add </h3>
<ul>
  <li>In "Other Options", add checkbox for textbox search being case-sensitive.</li>
  <li>Add options to open items from file.  This would be 1) a good feature and 2) finally a way to test AudioItem!  Only allow text files for text items, any file for file items, image files for image items, and audio files for audio items</li>
  <li>Maybe make MultiPaste slightly translucent?  Like ~90% opacity?</li>
  <li>Would like to make MultiPaste resizable</li>
  <li>Maybe give a custom color theme option, where the user can choose rgb values.  Would probably need to import a library to make it more interactive/easy to use (like a slider or something, with the color shown on the side).</li>
  <li>Create color theme option in config: dark or light mode.  Default would probably be light mode.  Could implement it by setting the color of the form, and then iterating through the Control.ControlCollection this.Controls to set each color.  Dark theme would be dark gray controls, white text; light theme would be light gray controls, black text.  I don't know how to change the window color but hopefully we can figure that out.  Would also like a fun theme that has a picture in the background or something.</li>
  <li>Option to edit saved items (start with text items); could be an option in a ContextMenuStrip for the listbox and/or via the Options dropdown</li>
  <li>When the listbox is moused over, show some additional information about the item at the selected index via tooltip text property</li>
  <li>Ctrl+z support to undelete items.  Could implement by storing the item in a temp ClipboardItem variable before deleting.  Then if Ctrl+z is pressed, it inserts that item back to the index at which it was deleted.  Would like more broad "undo" support if possible.</li>
  <li>Drag-and-drop support for raw images</li>
  <li>If copying an image, give user the option to paste as a file or image; config setting?</li>
  <li>Sync support via peer-to-peer relay server</li>
  <li>An improved look for the program icon</li>
  <li>Support for storing image as its original format</li>
</ul>
