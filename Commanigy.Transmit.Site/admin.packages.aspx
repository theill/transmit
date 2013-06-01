<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="admin.packages.aspx.cs"
	Inherits="Commanigy.Transmit.Site.AdminPackagesPage" Title="Packages" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<form id="form1" runat="server" class="admin packages">
		<h1><a href="admin.settings.aspx">Settings</a> <b>Packages</b></h1>
		
		<asp:ListView ID="LvPackages" runat="server">
		<LayoutTemplate>
			<table class="list">
			<tr>
				<th width="20%">Date</th>
				<th width="20%">Sender</th>
				<th>Files (first ten)</th>
				<th width="10%">Count</th>
			</tr>
			<tr runat="server" id="itemPlaceholder">
			</tr>
			</table>
		</LayoutTemplate>
		<ItemTemplate>
			<tr>
				<td><%# string.Format("{0:yyyy-MM-dd HH:mm:ss}", Eval("CreatedAt")) %></td>
				<td><%# Eval("SenderMail") %></td>
				<td><%# FilesList(Eval("Files")) %></td>
				<td><%# Eval("Files.Count") %></td>
			</tr>
		</ItemTemplate>
		</asp:ListView>
	</form>
</asp:Content>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		$(document).ready(function() {
		
		});
	</script>

</asp:Content>
