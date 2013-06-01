<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="request.success.aspx.cs" Inherits="Commanigy.Transmit.Site.RequestSuccessPage" Title="Successfully requested files" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<h1><%= _("Requested files") %></h1>
	<p>
		<%= _("You have successfully requested files.") %>
	</p>
</asp:Content>