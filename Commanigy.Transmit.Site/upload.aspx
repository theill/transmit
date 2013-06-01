<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="upload.aspx.cs" Inherits="Commanigy.Transmit.Site.UploadPage" Title="Upload files" %>
<%@ Register src="UploadControl.ascx" tagname="UploadControl" tagprefix="uc1" %>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">

	function GetToken() {
		return "<%= HfToken.Value %>";
	}

	$(document).ready(function() {
		Transmit.keepPageAlive();

		$(".avatar").attr("style", "background-image: url(<%= FormatProfilePicture() %>)");

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

			if (Transmit.fileCount() > 0) {
				$(this).attr("data-submitted", "true");
				$(this).html("Uploading files...");
				Transmit.startUploading();
				return false;
			}

			Transmit.showError("<%= _("You need to specify at least one file to share.") %>");
			return false;
		});
	});
	
</script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<form id="form1" runat="server" method="post" enctype="multipart/form-data" class="upload">
	<div class="grid_8 alpha">
		<asp:HiddenField ID="HfToken" runat="server" />
		<div id="profile">
			<div class="avatar"></div>
			<div class="display-name">
				<h1><span class="name"><%= this.Invitation.SenderDisplayName %></span></h1>
				<% if (!string.IsNullOrEmpty(this.Invitation.Message)) { %>
					<cite class="invitation-message"><asp:Label ID="LblInvitationMessage" runat="server" EnableViewState="false" /></cite>
				<% } else { %>
					<cite class="invitation-message"><%= _("would like you to upload files.") %></cite>
				<% } %>
			</div>			
			
			<div class="clear"></div>
		</div>
		<fieldset>
			<h3><span class="step"><%= _("Step 1") %></span> - <label for="FuUploads" class="step-desc"><%= _("Select files to upload") %></label> <span class="req">*</span></h3>
			<uc1:UploadControl ID="Uploader" runat="server" />
			<p class="hint"><%= _("You can upload unlimited number of files with maximum size of each file of <span class=\"filesize\">{0}</span>.", (Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.UploadSizeLimit / 1024).ToString() + "MB") %></p>
		</fieldset>
		
		<fieldset>
			<h3><span class="step"><%= _("Step 2") %></span> - <label for="TbxMessage" class="step-desc"><%= _("Type a message") %></label></h3>
			<div><asp:TextBox ID="TbxMessage" runat="server" TextMode="MultiLine" class="TbxMessage" /></div>
			<div class="hint"><%= _("Type an optional message to your recipients") %></div>
		</fieldset>
		
		<div class="big-button"><asp:LinkButton ID="LnkShare" runat="server" OnClick="BtnShare_Click" CssClass="submit-button upload-with-submit" /></div>
	</div>
	<div class="grid_4 omega">
		<div class="sidebar">
			<h2><%= _("Invited to share") %></h2>
			<p>
				<%= _("Congratulations! You have been asked to share one or more files. Complete step 1 and 2 and then click \"Upload Files\".") %>
			</p>
		</div>
	</div>
	</form>
</asp:Content>