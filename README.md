﻿<h1> MultiPaste </h1>
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
  <li>Moving item to the top or bottom should set index to nearby item rather than to the top or bottom</li>
  <li>After copying item, move index to that of the copied item</li>
  <li>ImageItem.IsEquivalent - with experience, I think it would be better to replace with new image if same size</li>
  <li>CustomItem.IsEquivalent</li>
  <li>Keep menuStrip open when a toggle check item is clicked.</li>
</ul>

<h3> Add </h3>
<ul>
  <li>Copy item with ctrl+c</li>
  <li>Ability to pin items to the top</li>
  <li>Search algorithm!</li>
  <li>Drag-and-drop support for raw images</li>
  <li>If copying an image, give user the option to paste as a file or image</li>
  <li>Sync support via peer-to-peer relay server</li>
  <li>An improved look for the program icon</li>
  <li>Support for storing image as its original format</li>
</ul>
