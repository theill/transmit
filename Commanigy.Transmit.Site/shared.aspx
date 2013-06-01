<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="shared.aspx.cs" Inherits="Commanigy.Transmit.Site.SharedPage" Title="Shared files" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<form id="form1" runat="server" class="share">
		<div class="grid_6 alpha">
			<fieldset>
				<h3><%= _("You shared this package with") %></h3>
				<ul>
					<asp:Repeater ID="SharedTo" runat="server">
						<ItemTemplate>
							<li><%# Eval("RecipientMail") %></li>
						</ItemTemplate>
					</asp:Repeater>
				</ul>
			</fieldset>

			<% if (!string.IsNullOrEmpty(this.Package.Message)) { %>
			<fieldset>
				<p class="message">
					&ldquo;<asp:Label ID="LblMessage" runat="server" EnableViewState="false" />&rdquo;
				</p>
			</fieldset>
			<% } %>
		</div>
		
		<div class="grid_6 omega">
			<div>
				<table width="100%">
				<tr>
					<th><%= _("Name") %></th>
					<th><%= _("Size") %></th>
					<th><%= _("Downloads") %></th>
				</tr>
				<asp:Repeater ID="DlFiles" runat="server">
				<ItemTemplate>
					<tr>
						<td><div class="filename"><%# Eval("FileHash") %></div></td>
						<td class="size"><div class="filesize"><%# Commanigy.Transmit.Web.NumberHelper.NumberToHumanSize(Convert.ToInt64(Eval("FileSize")), 0) %></div></td>
						<td class="download-count"><%# Eval("DownloadCount") %></td>
					</tr>
				</ItemTemplate>
				</asp:Repeater>
				</table>
			</div>
			
			<div>
				<asp:ImageButton ID="BtnDownload" runat="server" ImageUrl="~/images/download-files.png" onclick="BtnDownload_Click"  />
				<asp:Button ID="BtnReshare" runat="server" Text="Reshare" Enabled="false" Visible="false" onclick="BtnReshare_Click" />
			</div>
			
			<% if (this.Package.ExpiresAt.HasValue) { %>
				<div class="expires-at">
					<%= _("This package expires {0}.", this.Package.ExpiresAt.Value.ToString("D")) %>
				</div>
			<% } %>
		</div>
	</form>
</asp:Content>