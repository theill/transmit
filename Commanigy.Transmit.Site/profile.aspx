<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="profile.aspx.cs" Inherits="Commanigy.Transmit.Site.ProfilePage" %>

<asp:Content ID="Head1" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">
	$(function() {
		$("#profile .avatar").attr("style", "background-image: url(<%= FormatProfilePicture() %>)");
	});
</script> 
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<div class="grid_12 alpha omega">
		<div id="profile">
			<div class="avatar"></div>
			<div class="display-name">
				<h1><span class="name"><%= this.ProfileUser.DisplayName %></span> <div class="title-and-company"><span class="title"><%= this.ProfileUser.Title %></span> <% if (!string.IsNullOrEmpty(this.ProfileUser.Company)) { %><span class="at"><%= _("at") %></span> <span class="company-name"><%= this.ProfileUser.Company %></span><% } %></div></h1>
				
				<table class="profile-details">
					<tr>
						<th><%= _("Mail") %></th>
						<td><%= this.ProfileUser.Mail %>&nbsp;</td>
					</tr>
					<tr>
						<th><%= _("Location") %></th>
						<td><%= this.ProfileUser.Location %><% if (!string.IsNullOrEmpty(this.ProfileUser.Country)) { %>, <%= this.ProfileUser.Country%><% } %>&nbsp;</td>
					</tr>
					<tr>
						<th><%= _("Department") %></th>
						<td><%= this.ProfileUser.Department %>&nbsp;</td>
					</tr>
				</table>
			</div>
		</div>
	</div>
	
	<% if (this.CurrentUser == this.ProfileUser) { %>
		<div class="rule"></div>
		
		<div class="grid_6 alpha">
			<h3><%= _("Latest {0} you shared", Commanigy.Transmit.Web.StringHelper.Pluralize(DlSharedByUser.Items.Count, _("file"), _("files"))) %></h3>

			<% if (DlSharedByUser.Items.Count > 0) { %>
				<asp:Repeater ID="DlSharedByUser" runat="server">
				<ItemTemplate>
					<div class="alert shared">
						<%# _("<b>You</b> shared <a href=\"shared.aspx?h={0}\">{1}</a> on <abbr>{2}</abbr>", Eval("Package.Code"), FormatFiles(Eval("Package.Files")), string.Format("{0:MMMM d, yyyy}", Eval("CreatedAt"))) %>
						<p class="hint">
							<%# Commanigy.Transmit.Web.StringHelper.Truncate(Eval("Package.Message") as string, 40) %>
						</p>
					</div>
					<div class="separator"></div>
				</ItemTemplate>
				</asp:Repeater>
			<% }
		 else { %>
				<p>
					<a href="share.aspx"><%= _("Want to share a file?") %></a>
				</p>
			<% } %>
		</div>	
		<div class="grid_6 omega">
			<% if (DlRequestedFiles.Items.Count > 0) { %>
				<h3><%= _("Latest {0} you received", Commanigy.Transmit.Web.StringHelper.Pluralize(DlRequestedFiles.Items.Count, _("file"), _("files"))) %></h3>

				<asp:Repeater ID="DlRequestedFiles" runat="server">
				<ItemTemplate>
					<div class="alert received">
						<div class="user-picture">
							<img src="Pictures.ashx?p=images/no-user-picture.png&d=24&m=<%# Eval("Package.SenderMail") %>" alt="<%# Eval("Package.SenderMail") %>" />
						</div>
						<%# _("<b><a href=\"profile.aspx?id={0}\" title=\"{0}\">{1}</a></b> sent you <a href=\"receive.aspx?h={2}\">{3}</a> on <abbr>{4}</abbr>", Eval("Package.SenderMail"), TrimmedDisplayName(Eval("Package.SenderMail") as string), Eval("Package.Code"), FormatFiles(Eval("Package.Files")), string.Format("{0:MMMM d, yyyy}", Eval("CreatedAt"))) %>
						<p class="hint">
							<%# Commanigy.Transmit.Web.StringHelper.Truncate(Eval("Package.Message") as string, 40) %>
						</p>
					</div>
					<div class="rule"></div>
				</ItemTemplate>
				</asp:Repeater>
			
			<% }
		 else { %>
				<h3><%= _("You have not received any files yet") %></h3>
				<p>
					<a href="request.aspx"><%= _("Want to request a file?") %></a>
				</p>
			<% } %>
		</div>
		<div class="clear"></div>
		
		<div class="grid_6 alpha">
			<h3><%= _("Invitations you have sent") %></h3>
			<% if (DlInvitedUsers.Items.Count > 0) { %>
				<asp:Repeater ID="DlInvitedUsers" runat="server">
				<ItemTemplate>
					<div class="alert shared">
						<%# _("<b>You</b> invited <b>{0}</b> on <abbr>{1}</abbr>", Eval("RecipientDisplayName"), string.Format("{0:MMMM d, yyyy}", Eval("CreatedAt"))) %>
						<p class="hint">
							<%# Commanigy.Transmit.Web.StringHelper.Truncate(Eval("Message") as string, 40) %>
						</p>
					</div>
					<div class="separator"></div>
				</ItemTemplate>
				</asp:Repeater>
			<% }
		 else { %>
				<p>
					<%= _("You have not yet sent any invitations.") %>
				</p>
			<% } %>
		</div>
		
		<div class="grid_6">
		
		</div>
	<% } %>
</asp:Content>