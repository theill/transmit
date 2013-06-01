<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="request.aspx.cs" Inherits="Commanigy.Transmit.Site.RequestPage" Title="Request files" %>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">

	$(document).ready(function() {
		$("#TbxRecipients").focus();

		$(".submit-button").live("click", function() {
			if ($(this).attr("data-submitted") == "true") {
				return false;
			}

			if ($("input[name='Recipients[]']").length > 0) {
				$(this).attr("data-submitted", "true");
				return true;
			}

			Transmit.showError("<%= _("You need to specify at least one recipient.") %>");
			return false;
		});
	});
	
</script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<div id="help">
		<h2><%= _("How do I request a file?") %></h2>
		<div class="content">
			<ol>
				<li><%= _("Start by entering the name of the recipient (or recipients) you want to ask for a file. They will receive an email with an unique token allowing them to upload files to your private space.") %></li>
				<li><%= _("Once you have picked your recipients you may type an optional message for your recipients e.g. telling them which files you are interested in receiving.") %></li>
				<li><%= _("Finally click \"Request files\" and wait.") %></li>
			</ol>
		</div>
	</div>

	<form id="form1" runat="server">
		<div class="grid_8 alpha">
			<asp:ValidationSummary ID="ValidationSummary" runat="server" EnableViewState="False" />
			
			<fieldset>
				<h3><span class="step"><%= _("Step 1") %></span> - <label for="TbxRecipients" class="step-desc"><%= _("Specify recipients") %></label> <span class="req">*</span></h3>
				<div><input type="text" name="TbxRecipients" id="TbxRecipients" /></div>
				<div class="hint"><%= _("Enter names of your recipients. Click mouse or press ENTER to select recipient.") %></div>
			    
				<ul id="recipients" class="ac_results">
			    
				</ul>
			</fieldset>
			
			<fieldset>
				<h3><span class="step"><%= _("Step 2") %></span> - <label for="TbxMessage" class="step-desc"><%= _("Type a message") %></label></h3>
				<div><asp:TextBox ID="TbxMessage" runat="server" TextMode="MultiLine" class="TbxMessage" /></div>
				<div class="hint"><%= _("Tell user which file or files you want him to send you") %></div>
			</fieldset>
			
			<div class="big-button">
				<h3><%= _("Done?") %></h3>
				<asp:LinkButton ID="LnkRequest" runat="server" OnClick="BtnRequest_Click" CssClass="submit-button" />
			</div>			
		</div>
		<div class="grid_4 omega">
			<div class="sidebar">
				<h2><%= _("Requesting files") %></h2>

				<p>
					<%= _("You can request others to share one or more files with you.") %>
				</p>
				
				<h3><%= _("Request colleagues") %></h3>
				<p>
					<%= _("If you are requesting files to be shared by colleagues, entering their name is sufficient, as their email address will be looked up automatically.") %>
				</p>
				
				<h3><%= _("Request others") %></h3>
				<p>
					<%= _("Requesting files to be shared by others will require entering their full email address.") %>
				</p>
				<p>
					<%= _("Once you have sent off your request for file sharing, the recipient will receive an email notification with your request and a unique ID enabling him/her to upload the requested documents.") %>
				</p>
				<p>
					<%= _("When the files have been uploaded, you will receive an email notification.") %>
				</p>
				
				<p>
					<%= _("You need to complete step 1 and 2. Once you're done click \"Request files\".") %>
				</p>
				
				<p>
					<%= _("Steps marked with <span class=\"req\">*</span> are required.") %>
				</p>
			</div>
		</div>
	</form>
</asp:Content>