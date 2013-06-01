<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="receive.aspx.cs" Inherits="Commanigy.Transmit.Site.ReceivePage" Title="Receive files" %>
<%@ Import Namespace="Commanigy.Transmit.Web" %>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<form id="form1" runat="server" class="receive">
		<div class="grid_6 alpha">
			<fieldset>
				<div class="shared-by">
					<div class="avatar"><asp:Image ID="ImgSharedBy" runat="server" /></div>
					<div class="name"><asp:Label ID="LblSender" runat="server" /></div>
					<div class="clear"></div>
					
					<% if (!string.IsNullOrEmpty(this.Package.Message)) { %>
						<p class="message">
							&ldquo;<asp:Label ID="LblMessage" runat="server" EnableViewState="false" />&rdquo;
						</p>
					<% } %>
				</div>
			</fieldset>
		</div>
		
		<div class="grid_6 omega">
			<ul class="files">
				<asp:Repeater ID="DlFiles" runat="server">
				<ItemTemplate>
					<li class="file">
						<div class="filename"><%# Eval("FileHash") %></div>
						<div class="filesize"><%# NumberHelper.NumberToHumanSize((long)Eval("FileSize"), 0) %></div>
					</li>
				</ItemTemplate>
				</asp:Repeater>
			</ul>
			
			<% if (this.Package.Status == (char)Commanigy.Transmit.Data.PackageStatus.Open) { %>
				<asp:ImageButton ID="BtnDownload" runat="server" ImageUrl="~/images/download-files.png" onclick="BtnDownload_Click"  />
				
				<% if (this.Package.ExpiresAt.HasValue) { %>
					<div class="expires-at">
						<%= _("This package expires {0}.", this.Package.ExpiresAt.Value.ToString("D"))%>
					</div>
				<% } %>
			<% } else { %>
				<div class="expired">
					<%= _("This package has expired.") %>
				</div>
			<% } %>
		</div>
	</form>
</asp:Content>