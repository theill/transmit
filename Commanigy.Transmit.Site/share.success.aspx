<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="share.success.aspx.cs" Inherits="Commanigy.Transmit.Site.ShareSuccessPage" Title="Successfully shared files" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<h1><%= _("Shared files") %></h1>
	<p>
		<%= _("You have successfully shared your files.") %>
	</p>
</asp:Content>