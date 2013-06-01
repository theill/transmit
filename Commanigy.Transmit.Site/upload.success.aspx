<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="upload.success.aspx.cs" Inherits="Commanigy.Transmit.Site.UploadSuccessPage" Title="Successfully uploaded files" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<h1><%= _("Uploaded files") %></h1>
	<p>
		<%= _("You have successfully uploaded files.") %>
	</p>
</asp:Content>