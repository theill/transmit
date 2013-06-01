<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="admin.settings.aspx.cs" Inherits="Commanigy.Transmit.Site.AdminSettingsPage" Title="Settings" ValidateRequest="false" %>

<%@ Register assembly="System.Web.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" namespace="System.Web.UI.WebControls" tagprefix="asp" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript">
		$(function() {
			$("input[type=submit].submit").live("click", function() {
				$("#aspnetForm").attr("action", "admin.settings.aspx" + $("#tabs a.selected").attr("href"));
				return true;
			});
			
			function selectedTabFromLocation() {
				var id = location.href.split('#')[1];
				if (id) {
					id = id.split('?')[0];
				}
				
				return id ? id : 0;
			}
			
			$("#tabs").idTabs(function(id, list, set, settings) {
				$("a", set).removeClass("selected").filter("[href='"+id+"']", set).addClass("selected");
				for (i in list) { $(list[i]).hide(); }
				$(id).fadeIn();
				return false;
			}, { start: selectedTabFromLocation() });

			$(".toggle-preview").click(function() {
				var fs = $(this).closest("fieldset");
				fs.find(".preview").html('<div style="border: ridge 1px #ccc">' + fs.find("textarea").val() + '</div>').toggle();
				fs.find("textarea, p.hint").toggle();
				return false;
			});

		});
	</script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<div class="grid_8 alpha">
		<form id="form1" runat="server">
			<div id="settings">
				<asp:FormView runat="server" DataKeyNames="ID" DataSourceID="LdsSettings" DefaultMode="Edit" EnableModelValidation="True">
					<EditItemTemplate>
					
  					<div id="t1" style="display: none;">
  						<h2>Site &amp; access</h2>
						<fieldset>
							<asp:Label ID="LblCompanyNameTextBox" AssociatedControlID="CompanyNameTextBox" runat="server">Company Name</asp:Label>
							<asp:TextBox ID="CompanyNameTextBox" runat="server" Text='<%# Bind("CompanyName") %>' Columns="60" />
							<p class="hint">Name of company displayed in copyright-text, default mail messages, etc.</p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblCompanyLogoTextBox" AssociatedControlID="CompanyLogoTextBox" runat="server">Company Logo</asp:Label>
							<asp:TextBox ID="CompanyLogoTextBox" runat="server" Text='<%# Bind("CompanyLogo") %>' Columns="60" />
							<p class="hint">Full URL for company logo. Must be a PNG, GIF or JPG with a size of 200x48.</p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblInternalUrlTextBox" AssociatedControlID="InternalUrlTextBox" runat="server">Internal Url</asp:Label>
							<asp:TextBox ID="InternalUrlTextBox" runat="server" Text='<%# Bind("InternalUrl") %>' Columns="60" />
							<p class="hint">URL used by internal staff when sharing files over LAN.</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblExternalUrlTextBox" AssociatedControlID="ExternalUrlTextBox" runat="server">External Url</asp:Label>
							<asp:TextBox ID="ExternalUrlTextBox" runat="server" Text='<%# Bind("ExternalUrl") %>' Columns="60" />
							<p class="hint">URL used externally when requesting files from customers or other people not on your LAN.</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblRestrictSettingsToGroup" AssociatedControlID="TbxRestrictSettingsToGroup" runat="server">Access to Settings page</asp:Label>
							<asp:TextBox ID="TbxRestrictSettingsToGroup" runat="server" Text='<%# Bind("RestrictSettingsToGroup") %>' Columns="60" />
							<p class="hint">Local groups of users that should have access to configuration settings (e.g. "SERVER1\Transmit Administrators, SERVER1\Administrators").</p>
						</fieldset>
					</div>
					
					<div id="storage" style="display: none;">
						<h2>Storage</h2>
						<fieldset>
							<asp:Label ID="LblUploadUrlTextBox" AssociatedControlID="UploadUrlTextBox" runat="server">Upload Url</asp:Label>
							<asp:TextBox ID="UploadUrlTextBox" runat="server" Text='<%# Bind("UploadUrl") %>' Columns="60" />
							<p class="hint">URL used when uploading files. Must be placed in DMZ to allow Flash plugin to communicate with it.</p>
						</fieldset>

						<fieldset>
							<asp:CheckBox ID="CbUploadChunked" runat="server" Checked='<%# Bind("UploadChunked") %>' Text="Use Chunked Uploads" />
							<p class="hint">Chunked uploads uses a Java Applet to resubmit lost packages. Should be used for large uploads on slow networks.</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblStorageLocationTextBox" AssociatedControlID="StorageLocationTextBox" runat="server">Storage Location</asp:Label>
							<asp:TextBox ID="StorageLocationTextBox" runat="server" Text='<%# Bind("StorageLocation") %>' Columns="60" />
							<p class="hint">Network share or local storage where all uploaded files are stored.</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblUploadSizeLimit" AssociatedControlID="TbxUploadSizeLimit" runat="server">Upload filesize limit</asp:Label>
							<asp:TextBox ID="TbxUploadSizeLimit" runat="server" Text='<%# Bind("UploadSizeLimit") %>' Columns="60" />
							<p class="hint">Maximum size of uploaded files in kilobytes.</p>
						</fieldset>
					</div>
					
					<div id="t2" style="display: none;">	
						<h2>LDAP</h2>

						<fieldset>
							<asp:Label ID="LblLdapSizeLimit" AssociatedControlID="TbxLdapSizeLimit" runat="server">Query result limit</asp:Label>
							<asp:TextBox ID="TbxLdapSizeLimit" runat="server" Text='<%# Bind("LdapSizeLimit") %>' Columns="60" />
							<p class="hint">Maximum number of results returned per LDAP query.</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblLdapFilterNameTextBox" AssociatedControlID="LdapFilterNameTextBox" runat="server">Name query</asp:Label>
							<asp:TextBox ID="LdapFilterNameTextBox" runat="server" Text='<%# Bind("LdapFilterName") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">LDAP query executed when searching for employees based on name.</p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblLdapFilterMailTextBox" AssociatedControlID="LdapFilterMailTextBox" runat="server">Email query</asp:Label>
							<asp:TextBox ID="LdapFilterMailTextBox" runat="server" Text='<%# Bind("LdapFilterMail") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">LDAP query executed when searching for employees based on email.</p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblLdapFilterLoginTextBox" AssociatedControlID="LdapFilterLoginTextBox" runat="server">Login query</asp:Label>
							<asp:TextBox ID="LdapFilterLoginTextBox" runat="server" Text='<%# Bind("LdapFilterLogin") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">LDAP query executed when searching for employees based on login.</p>
						</fieldset>
					</div>
					
					<div id="t3" style="display: none;">
						<h2>Mail server</h2>

						<fieldset>
							<asp:CheckBox ID="CbMailSecure" runat="server" Checked='<%# Bind("MailSecure") %>' Text="Use SSL" />
							<p class="hint"></p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblMailReplyToTextBox" AssociatedControlID="MailReplyToTextBox" runat="server">Reply-to address</asp:Label>
							<asp:TextBox ID="MailReplyToTextBox" runat="server" Text='<%# Bind("MailReplyTo") %>' Columns="60" />
							<p class="hint">Force reply-address for all outgoing emails e.g. "no-reply@company.com". If empty, uses "from" address.</p>
						</fieldset>
					</div>
					
					<div id="t4" style="display: none;">
						<h2>Mailing</h2>
						
						<h3>Sharing</h3>
						<fieldset>
							<asp:Label ID="LblShareMailSubjectTextBox" AssociatedControlID="ShareMailSubjectTextBox" runat="server">Subject</asp:Label>
							<asp:TextBox ID="ShareMailSubjectTextBox" runat="server" Text='<%# Bind("ShareMailSubject") %>' Columns="60" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Package.Code, Package.Files.Count, Mail.Message, File.Location</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblShareMailBodyPlainTextBox" AssociatedControlID="ShareMailBodyPlainTextBox" runat="server">Body, Text</asp:Label>
							<asp:TextBox ID="ShareMailBodyPlainTextBox" runat="server" Text='<%# Bind("ShareMailBodyPlain") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Package.Code, Package.Files.Count, Mail.Message, File.Location</p>
						</fieldset>

						<fieldset id="share-mail-body-html">
							<asp:Label ID="LblShareMailBodyHtmlTextBox" AssociatedControlID="ShareMailBodyHtmlTextBox" runat="server">Body, HTML - <a href="#" class="toggle-preview">Toggle Mail Preview</a></asp:Label>
							<asp:TextBox ID="ShareMailBodyHtmlTextBox" runat="server" Text='<%# Bind("ShareMailBodyHtml") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Package.Code, Package.Files.Count, Mail.Message, File.Location</p>
							<div class="preview" style="display: none"></div>
						</fieldset>

						<h3>Request</h3>
						<fieldset>
							<asp:Label ID="LblRequestMailSubjectTextBox" AssociatedControlID="RequestMailSubjectTextBox" runat="server">Subject</asp:Label>
							<asp:TextBox ID="RequestMailSubjectTextBox" runat="server" Text='<%# Bind("RequestMailSubject") %>' Columns="60" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Mail.Message, Mail.InvitationCode, Url.Location</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblRequestMailBodyPlainTextBox" AssociatedControlID="RequestMailBodyPlainTextBox" runat="server">Body, Text</asp:Label>
							<asp:TextBox ID="RequestMailBodyPlainTextBox" runat="server" Text='<%# Bind("RequestMailBodyPlain") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Mail.Message, Mail.InvitationCode, Url.Location</p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblRequestMailBodyHtmlTextBox" AssociatedControlID="RequestMailBodyHtmlTextBox" runat="server">Body, HTML - <a href="#" class="toggle-preview">Toggle Mail Preview</a></asp:Label>
							<asp:TextBox ID="RequestMailBodyHtmlTextBox" runat="server" Text='<%# Bind("RequestMailBodyHtml") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Mail.Message, Mail.InvitationCode, Url.Location</p>
							<div class="preview" style="display: none"></div>
						</fieldset>

						<h3>Upload</h3>
						<fieldset>
							<asp:Label ID="LblUploadMailSubjectTextBox" AssociatedControlID="UploadMailSubjectTextBox" runat="server">Subject</asp:Label>
							<asp:TextBox ID="UploadMailSubjectTextBox" runat="server" Text='<%# Bind("UploadMailSubject") %>' Columns="60" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Mail.Message, File.Location</p>
						</fieldset>
						
						<fieldset>
							<asp:Label ID="LblUploadMailBodyPlainTextBox" AssociatedControlID="UploadMailBodyPlainTextBox" runat="server">Body, Text</asp:Label>
							<asp:TextBox ID="UploadMailBodyPlainTextBox" runat="server" Text='<%# Bind("UploadMailBodyPlain") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Mail.Message, File.Location</p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblUploadMailBodyHtmlTextBox" AssociatedControlID="UploadMailBodyHtmlTextBox" runat="server">Body, HTML - <a href="#" class="toggle-preview">Toggle Mail Preview</a></asp:Label>
							<asp:TextBox ID="UploadMailBodyHtmlTextBox" runat="server" Text='<%# Bind("UploadMailBodyHtml") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">Supported tokens: Sender.Mail, Sender.DisplayName, Recipient.Mail, Recipient.DisplayName, Mail.Message, File.Location</p>
							<div class="preview" style="display: none"></div>
						</fieldset>
						
						<h3>Outlook</h3>
						<fieldset>
							<asp:Label ID="LblOutlookInjectedHtmlTextBox" AssociatedControlID="OutlookInjectedHtmlTextBox" runat="server">HTML - <a href="#" class="toggle-preview">Toggle Mail Preview</a></asp:Label>
							<asp:TextBox ID="OutlookInjectedHtmlTextBox" runat="server" Text='<%# Bind("OutlookInjectedHtml") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint">Supported tokens: Package.Code, TransmitUrl</p>
							<div class="preview" style="display: none"></div>
						</fieldset>
					</div>

					<div id="t5" style="display: none;">
						<h2>Messages</h2>
						<p>
							Default text messages displayed for users when using web functionality.
						</p>
						<fieldset>
							<asp:Label ID="LblShareDefaultMessageTextBox" AssociatedControlID="ShareDefaultMessageTextBox" runat="server">Share</asp:Label>
							<asp:TextBox ID="ShareDefaultMessageTextBox" runat="server" Text='<%# Bind("ShareDefaultMessage") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint"></p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblRequestDefaultMessageTextBox" AssociatedControlID="RequestDefaultMessageTextBox" runat="server">Request</asp:Label>
							<asp:TextBox ID="RequestDefaultMessageTextBox" runat="server" Text='<%# Bind("RequestDefaultMessage") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint"></p>
						</fieldset>

						<fieldset>
							<asp:Label ID="LblUploadDefaultMessageTextBox" AssociatedControlID="UploadDefaultMessageTextBox" runat="server">Upload</asp:Label>
							<asp:TextBox ID="UploadDefaultMessageTextBox" runat="server" Text='<%# Bind("UploadDefaultMessage") %>' Rows="4" Columns="60" TextMode="MultiLine" />
							<p class="hint"></p>
						</fieldset>
					</div>
						
						<fieldset>
							<asp:Button ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" Text="Save" CssClass="submit-button" />
						</fieldset>
					</EditItemTemplate>
				</asp:FormView>
			</div>
			
			<asp:LinqDataSource ID="LdsSettings" runat="server" 
				ContextTypeName="Commanigy.Transmit.Data.DataClassesDataContext" 
				TableName="Settings" EnableUpdate="True" OrderBy="CreatedAt desc" OnUpdated="Settings_Updated" OnUpdating="Settings_Updating">
			</asp:LinqDataSource>
		</form>
	</div>
	
	<div class="grid_4 omega">
		<div class="sidebar">
			<h2>Configuration</h2>
			<ul id="tabs">
				<li><a href="#t1">Site &amp; access</a></li>
				<li><a href="#storage">Storage </a></li>
				<li><a href="#t2">LDAP</a></li>
				<li><a href="#t3">Mail server</a></li>
				<li><a href="#t4">Mailing</a></li>
				<li><a href="#t5">Messages</a></li>
			</ul>
			
			<h2>Administrative Tasks</h2>
			<ul>
				<li><a href="admin.cleanup.aspx">Cleanup files</a></li>
				<li><a href="admin.logs.aspx">Logs</a></li>
				<li><a href="admin.packages.aspx">Packages</a></li>
				<li><a href="admin.users.aspx">Users</a></li>
			</ul>
		</div>
	</div>
</asp:Content>