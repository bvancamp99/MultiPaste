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

<h3> Fix </h3>
<ul>
  <li>AudioItem is implemented, but a method of testing it is yet to be found.  Stream.Length might serve as a problem, since its documentation seems to imply that it's only supported in derived classes that use Length.</li>
</ul>

<h3> Improve </h3>
<ul>
  <li>Could potentially conflate IsEquivalent on ImageItem, AudioItem, and CustomItem.  Would need to create file before comparing the ClipboardItems (if possible).  Then we would compare byte length of the files first, then maybe do more checking if that bool returns true (although byte-by-byte checking could become VERY slow).</li>
  <li>Not sure if this would be possible, but making MainWindow static would make a lot of sense.  Then we don't have to pass copies of its instance around to every class.  Issue is then it can't inherit from the Form class (which isn't static).</li>
  <li>ushort -> int; simpler that way</li>
  <li>Properties for the custom classes in MainWindow.  Then we have a clean way for most classes to safely access each other.</li>
  <li>Fewer static globals; make them non-static and/or replace with MainWindow instance</li>
  <li>Move RestrictTypes to MyClipboard once we get the MainWindow instances passed</li>
  <li>Properties instead of getters</li>
  <li>Pass MainWindow instances</li>
  <li>So we init instances of our custom classes in MainWindow, right?  Why don't we make use of those instances for methods in those classes only used by MainWindow?  Then we would need fewer static methods.</li>
  <li>Use CheckedChanged event for updating config items</li>
  <li>See about removing getters from LocalClipboard.cs</li>
  <li>Would like to improve ClipboardItem.SetKeyDiff.  It does a lot of things that don't make sense without significant context.  Would also like to move it to LocalClipboard.cs</li>
  <li>Show copied images on the listbox</li>
  <li>Could just compare the byte arrays to determine equivalence for ImageItem, AudioItem, and CustomItem</li>
  <li>Merge all ClipboardItem classes into one class</li>
  <li>Class for config file</li>
  <li>Work on MsgLabel.Fatal</li>
  <li>Catch errors thrown and write to an error log file.  Could be like in CS 536's ErrMsg.java file, which has static methods fatal and warning.  If warning, write to error log and notify user.  If failure, prog would probably have to close, but the user can refer to the error log for details.  Should display the error, along with line num of the CLIPBOARD file; maybe some additional things.</li>
  <li>Config setting for up/down keys wrapping to the other end</li>
  <li>Moving item to the top or bottom should set index to nearby item rather than to the top or bottom.  Maybe provide a config setting to let the user choose.</li>
  <li>After copying item, move index to that of the copied item; config setting?</li>
  <li>ImageItem.IsEquivalent - with experience, I think it would be better to replace with new image if same size</li>
  <li>CustomItem.IsEquivalent</li>
  <li>Keep menuStrip open when a toggle check item is clicked.</li>
</ul>

<h3> Add </h3>
<ul>
  <li>Maybe add class for help file</li>
  <li>Ctrl+z support to undelete items.  Could implement by storing the item in a temp ClipboardItem variable before deleting.  Then if Ctrl+z is pressed, it pushes that item to the top of the list.</li>
  <li>Copy item with ctrl+c</li>
  <li>Ability to pin items to the top</li>
  <li>Search algorithm!</li>
  <li>Drag-and-drop support for raw images</li>
  <li>If copying an image, give user the option to paste as a file or image; config setting?</li>
  <li>Sync support via peer-to-peer relay server</li>
  <li>An improved look for the program icon</li>
  <li>Support for storing image as its original format</li>
</ul>
