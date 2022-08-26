Recommended extra steps after an installation of LPCXpresso
--------------------------------------------------------------------------------
To be done in the given order. All locally referenced files are siblings to this
file.

Help > Install New Software...
  Work with: Eclipse Marketplace Client
  Check: EPP Marketplace Client
  If the marketplace cannot be found, you'll need to add Eclipse marketplace Neon - http://download.eclipse.org/releases/neon

Help > Eclipse Marketplace...
  Find (& install):
    Path Tools
    pydev
    Bracketeer for C/C++ (CDT)
    Subversive - SVN Team Provider
  
After restart, when prompted for a subversion connector (could be immediately, could pop up at a later moment),
select SVN Kit 1.8.x.

Help > Install New Software...
  Add... > Archive:
    <sensor_nss_firmware>/trunk/tools/lpcxpresso/plugins/com.nxp.nhs31xx.*.zip
  Uncheck 'Contact all update sites during install to find software' to speed up
  the installation process.

File > Import...: General > Preferences
  From preference file:
    <sensor_nss_firmware>/trunk/tools/lpcxpresso/settings/nss_sw_preferences.epf

Window > Preferences: General > Editors > Text Editors > Spelling
  For user defined dictionary:
    <sensor_nss_firmware>/trunk/tools/lpcxpresso/settings/nss_sw_dictionary.txt

Window > Preferences: C/C++ > Editor
  Select Doxygen as Workspace default under Documentation tool comments.

Window > Preferences: C/C++ > Code Style > Formatter
  Import:
    <sensor_nss_firmware>/trunk/tools/lpcxpresso/settings/nss_sw_formatter_settings.xml

Window > Preferences: C/C++ > Code Analysis > No break at end of case
    Enter "fallthrough" as comment text to suppress the problem.
  
Edit <LPCXpresso>/lpcxpresso/lpcxpresso.ini
  Compare with and adapt from:
    <sensor_nss_firmware>/trunk/tools/lpcxpresso/settings/lpcxpresso.ini
  For a rationale, check:
    https://piotrga.wordpress.com/2006/12/12/intellij-and-garbage-collection/
    http://howtodoinjava.com/2014/04/05/how-to-quickly-make-eclipse-faster/ 
