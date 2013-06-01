<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="share.aspx.cs" Inherits="Commanigy.Transmit.Site.SharePage" Title="Share files" %>
<%@ Register src="UploadControl.ascx" tagname="UploadControl" tagprefix="uc1" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">

	function GetToken() {
		return "<%= HfToken.Value %>";
	}

	$(function() {
		Transmit.keepPageAlive();
		
		$("#TbxRecipients").focus();

		$("#TbxRecipients").keydown(function(e) {
			if (e.keyCode == '13') {
				e.preventDefault();
			}
		});
		
		$(".submit-button").live("click", function() {
			if ($(this).attr("data-uploaded") == "true") {
				return true;
			}

			if ($(this).attr("data-submitted") == "true") {
				return false;
			}

			if ($("input[name='Recipients[]']").length > 0 && Transmit.fileCount() > 0) {
				$(this).attr("data-submitted", "true");
				$(this).html("Uploading files...");
				Transmit.startUploading();
				return false;
			}

			$("#uploader-queue").addClass("warning");

			Transmit.showError('<%= _("You need to specify at least one recipient and one file to share.") %>');
			return false;
		});

	});
	
</script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<div id="help">
		<h2><%= _("How do I share a file?") %></h2>
		<div class="content">
			<p>
				<%= _("It is possible to share one or more files with your co-workers or clients. Once uploaded they will be secured internally and only the recipients you specify will be able to access them.") %>
			</p>
			<ol>
				<li><%= _("Click \"Add files\" to select one or more files you want to share.") %></li>
				<li><%= _("While these files are uploading, enter a name of the recipient you want to send these files. You may add as many recipients as necessary. They will all receive an email with an unique token allowing them to download your files.") %></li>
				<li><%= _("Optionally you may enter a message for all recipients e.g. a reason for sharing these files.") %></li>
				<li><%= _("Complete sharing by clicking \"Share files\" button. If your files still are uploading the page will wait till its completed.") %></li>
			</ol>
		</div>
	</div>
	<form id="FrmShare" runat="server" method="post" enctype="multipart/form-data" class="share">
		<asp:HiddenField ID="HfToken" runat="server" />
		<div class="grid_8 alpha">
			<asp:ValidationSummary ID="ValidationSummary" runat="server" EnableViewState="False" />
			
			<fieldset>
				<h3><span class="step"><%= _("Step 1") %></span> - <label for="FuUploads" class="step-desc"><%= _("Select files to share") %></label> <span class="req">*</span></h3>
				<uc1:UploadControl ID="Uploader" runat="server" />
				<p class="hint"><%= _("You can upload unlimited number of files with maximum size of each file of <span class=\"filesize\">{0}</span>.", (Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.UploadSizeLimit / 1024).ToString() + "MB") %></p>
			</fieldset>
			
			<fieldset>
				<h3><span class="step"><%= _("Step 2") %></span> - <label for="TbxRecipients" class="step-desc"><%= _("Specify recipients while files are being uploaded") %></label> <span class="req">*</span></h3>
				<div><input type="text" name="TbxRecipients" id="TbxRecipients" /></div>
				<div class="hint"><%= _("Enter names of your recipients. Click mouse or press ENTER to select recipient.") %></div>
			    
				<ul id="recipients" class="ac_results">
			   
				</ul>
			</fieldset>
			
			<fieldset>
				<h3><span class="step"><%= _("Step 3") %></span> - <label for="TbxMessage" class="step-desc">Type a message</label></h3>
				<div><asp:TextBox ID="TbxMessage" class="TbxMessage" runat="server" TextMode="MultiLine" /></div>
				<div class="hint"><%= _("Type an optional message to your recipients") %></div>
			</fieldset>
			
			<div class="big-button">
				<h3>Done?</h3>
				<asp:LinkButton ID="LnkShare" runat="server" OnClick="BtnShare_Click" CssClass="submit-button upload-with-submit" />
			</div>
		</div>
		<div class="grid_4 omega">
			<div class="sidebar">
				<h2><%= _("Sharing files") %></h2>
				<p>
					<%= _("You can share one or more large files with others.") %>
				</p>
					
				<h3><%= _("Sharing with colleagues") %></h3>
				<p>
					<%= _("If you are sharing files with colleagues, entering their name is sufficient, as their email address will be looked up automatically.") %>
				</p>
				
				<h3><%= _("Sharing with others") %></h3>
				<p>
					<%= _("Sharing files with others will require entering their full email address.") %>
				</p>
				<p>
					<%= _("The recipients will automatically receive an email notification with your message enabling them to access the files.") %>
				</p>
				
				<p>
					<%= _("You need to complete step 1, 2 and 3. Once you're done click \"Share files\".") %>
				</p>
				<p>
					<%= _("Steps marked with <span class=\"req\">*</span> are required.") %>
				</p>
			</div>
		</div>
	</form>
</asp:Content>