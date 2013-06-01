<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="authenticate.aspx.cs" Inherits="Commanigy.Transmit.Site.AuthenticatePage" Title="Authentication failed" %>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server"></asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<h1><%= _("Authentication failed") %></h1>
	<p>
		<%= _("You are not allowed to perform requested action.") %>
	</p>
</asp:Content>
