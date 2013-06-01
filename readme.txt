mstsc /v:80.196.100.70
adminpt/4rfv5tgb


run 'pack.cmd' to pack files so they are ready to be deployed

= Migration scripts

To run migrations, do this:

osql -U sa -P 123456 -d transmit_development -i 001_add_company_url_to_settings.sql


= Backup data in database

 * Right-click database in "Server Explorer"
 * Select "Publish to provider..."
 * Save in file

= Update DBML

* Remove "Connection" from VS.NET
* Replace with "transmitConnectionString (Settings)"

= Localization

Set language on a page using e.g.

			UICulture = "da";
			Culture = "da-DK";	

			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("da");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("da");
