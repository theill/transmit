<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="admin.cleanup.aspx.cs" Inherits="Commanigy.Transmit.Site.AdminCleanupPage" Title="Cleanup" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<div class="admin cleanup">
		<h1><a href="admin.settings.aspx">Settings</a> <b>Cleanup files</b></h1>

		<form id="Form1" runat="server">
		
		<h2>Expired packages</h2>
		<% if (DlExpiredPackages.Items.Count > 0) { %>
			<p>
				A total number of <%= Commanigy.Transmit.Web.StringHelper.Pluralize(DlExpiredPackages.Items.Count, "package", "packages")%>
				have expired. You may clean them up now.
			</p>
			<ul>
				<asp:Repeater ID="DlExpiredPackages" runat="server">
				<ItemTemplate>
					<li><%# Eval("Code") %></li>
				</ItemTemplate>
				</asp:Repeater>
			</ul>

			<p>
				<asp:Button ID="BtnCleanup" runat="server" Text="Cleanup now" onclick="BtnCleanup_Click" />
			</p>
		<% } else { %>
			<p>
				No packages have expired.
			</p>
		<% } %>
		
		<h2>Partial packages</h2>
		<% if (DlAvailableDirectories.Items.Count > 0) { %>
			<ul>
			<asp:Repeater ID="DlAvailableDirectories" runat="server">
				<ItemTemplate>
					<li><%# Container.DataItem %></li>
				</ItemTemplate>
			</asp:Repeater>
			</ul>
			
			<asp:Button ID="BtnDeleteUnattachedPackages" runat="server" Text="Cleanup partial packages" onclick="BtnDeleteUnattachedPackages_Click" />
		<% } else { %>
			<p>
				No partial packages.
			</p>		
		<% } %>
		</form>
		
	</div>
</asp:Content>