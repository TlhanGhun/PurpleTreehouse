PurpleTreehouse is a Snarl (http://www.fullphat.net/) application which gives you the ability to use Snarl to subsribe to a central news provider. It uses a defined XML structure on the central location (either a network file path or a http address) to retrieve the news and displays to the local running Snarl application.

It is configured by one config file next to the application executable. It contains the following parameters:

UrlOrUnc: Address of the XML file (UNC, local path or http based)

AppIcon: Path to the icon used for registering with Snarl (UNC, local path or http based)

Enable Settings: not used until now - later on user settings can be switched on with this parameter

UpdateInterval: Interval in seconds to check for new items

LastIdShown: Memorizes the highest id already shown to this user. This value is saved in %LOCALAPPDATA% per user. Only alerts with a higher id than the already shown are displayed

DefaultNotificationIcon: If the XML does not contain an icon use this on (UNC, local path or http based)

DefaultDisplayTime: If the XML does not contain a display time use this on

DefaultNotificationClass: If the XML does not contain a notification class use this on

ListOfNotificationClasses: ;-separated list of notification classes (like "Alert;Company success news;IT news"

AppName: The name which shall be used when registering with Snarl

ShowErrorNotifications: (True/False) Gives notifications in the extra Alert class "Debug" like "File not found" our "XML parsing error"

ResetAlreadySeenIds: Resets LastIdShown (see above) - useful for testing ;) 

Here an example XML:

<alerts> 
 <alert> 
  <id>1</id>
  <title>Click me to go to my homepage</title> 
  <text>Some
multiline
text</text> 
  <icon>http://tlhan-ghun.de/favicon.ico</icon> <!-- if not given default is used - e.g. company logo --> 
  <class>AlertClass to use</class> 
  <leftClickUrl>http://tlhan-ghun.de</leftClickUrl> 
  <rightClickUrl>http://server/page.php</rightClickUrl> 
  <middleClickUrl>http://server/page.php</middleClickUrl>  <!-- those three define what shall happen if the user clicks the notification -->
 </alert> 
 <alert> 
  <id>2</id> <!-- to avoid double notification on new start last already send is memorized --> 
  <title>Another text</title> 
  <text>Some
other
multiline
text</text> 
  <icon>http://tlhan-ghun.de/favicon.ico</icon> 
  <class>AlertClass to use</class> 
  <leftClickUrl>http://server/page.php</leftClickUrl> 
  <rightClickUrl>http://server/page.php</rightClickUrl> 
  <middleClickUrl>http://server/page.php</middleClickUrl> 
 </alert>
</alerts>
