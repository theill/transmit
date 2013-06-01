<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="admin.logs.aspx.cs"
	Inherits="Commanigy.Transmit.Site.AdminLogsPage" Title="Logs" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<form id="form1" runat="server" class="admin logs">
		<h1><a href="admin.settings.aspx">Settings</a> <b>Logs</b></h1>
		
		<% if (DlLatestLogs.Items.Count > 0) { %>
		<div id="logs">
			<asp:ListView ID="DlLatestLogs" runat="server">
				<LayoutTemplate>
					<table id="logs-overview">
						<tr runat="server" id="itemPlaceholder">
						</tr>
					</table>
				</LayoutTemplate>
				<ItemTemplate>
					<tr>
						<td class="created_at">
							<%# FormatToggleImage(Eval("Exception")) %>
							<%# string.Format("{0:yyyy-MM-dd HH:mm:ss}", Eval("CreatedAt"))%>
						</td>
						<td class="level">
							<%# FormatLevel(Eval("Level") as string) %>
						</td>
						<td class="detail">
							<div><%# Eval("Message") %></div>
							<div class="exception">
								<pre><%# Eval("Exception") %></pre>
							</div>
						</td>
					</tr>
				</ItemTemplate>
			</asp:ListView>
			<div class="pager">
			<div id="legend">
				<ul>
					<li>
						<div class="level debug">D</div>&nbsp;Debug (least severe)</li>
					<li>
						<div class="level info">I</div>&nbsp;Info</li>
					<li>
						<div class="level warn">W</div>&nbsp;Warning</li>
					<li>
						<div class="level error">E</div>&nbsp;Error</li>
					<li>
						<div class="level fatal">F</div>&nbsp;Fatal (most severe)</li>
				</ul>
			</div>
				Page
				<asp:DataPager ID="DataPager1" runat="server" PagedControlID="DlLatestLogs" PageSize="25">
					<Fields>
						<asp:NumericPagerField ButtonCount="3" NextPageText=">" PreviousPageText="<" CurrentPageLabelCssClass="current" />
					</Fields>
				</asp:DataPager>
			</div>
		</div>
		<% }
	 else { %>
		<p>
			No logs available.
		</p>
		<% } %>
	</form>
</asp:Content>
<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">

	<script type="text/javascript">
		$(document).ready(function() {
			$(".toggle-exception").live("click", function() {
				$(this).closest("tr").find(".exception").toggle();
				url = $(this).find("img").attr("src");
				if (url.indexOf("plus") != -1) {
					url = url.replace('plus', 'minus');
				}
				else {
					url = url.replace('minus', 'plus');
				}
				$(this).find("img").attr("src", url);
				return false;
			});
		});
	</script>

</asp:Content>
