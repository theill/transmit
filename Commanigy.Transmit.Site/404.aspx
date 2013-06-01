<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="Commanigy.Transmit.Site.Four04Page" Title="404 - File not found" %>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<h1><%= _("Resource not found") %></h1>
	<p>
		<%= _("Requested resource was not found.") %>
	</p>
</asp:Content>