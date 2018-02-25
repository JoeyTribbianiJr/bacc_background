﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Bacc_background
{
	public class SettingItem
	{
		//显示在菜单上的描述
		public int SelectedIndex{ get; set; }

		public SettingItemType Type { get; set; }
		//item的所有可选值
		public string[] Values { get; set; }
	}
	public enum PlayType
	{
		single,
		two
	}
	public enum SettingItemType
	{
		intager,
		strings
	}
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public Dictionary<string, SettingItem> app_setting = new Dictionary<string, SettingItem>();
		public Dictionary<string, SettingItem> game_setting = new Dictionary<string, SettingItem>();

		public MainWindow()
		{
			InitSetting();
			InitializeComponent();
			lstButton.ItemsSource = game_setting;
			//lstButtonInt.ItemsSource = game_setting;
		}
		private void InitSetting()
		{
			game_setting.Add("printer", new SettingItem() {
				SelectedIndex =1,
				Type= SettingItemType.intager,
				Values = new string[2] { "热敏打印机:1", "热敏打印机:2" }});
			
			game_setting.Add("round_num_per_session", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "一场局数:30","一场局数:45","一场局数:66"}});
		
			game_setting.Add("check_waybill_tm", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "对单时间:30","对单时间:60" }});

			game_setting.Add("bet_tm", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "押分时间:30", "押分时间:60" } });
			//game_setting.Add("shuffle_tm", new SettingItem() { desc = "洗牌时间", value = 120, values = new int[] { 30, 60 } });

			game_setting.Add("big_chip_facevalue", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "大筹码:100", "大筹码:500", "大筹码:1000" } });

			game_setting.Add("mini_chip_facevalue", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "小筹码:1", "小筹码:10","小筹码:100"} });

			game_setting.Add("total_limit_red", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "总限红:3000", "总限红:5000"} });

			game_setting.Add("desk_limit_red", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "单台限红:3000","单台限红:5000","单台限红:30000" } });

			game_setting.Add("tie_limit_red", new SettingItem() {
				SelectedIndex = 0,
				Type = SettingItemType.intager,
				Values = new string[] { "和限红:3000","和限红:5000","和限红:30000" } });

			//game_str_setting.Add("bgm_on", new SettingStrItem() { desc = "背景音乐开关", value = "背景音乐开", values = new string[] { "背景音乐开", "背景音乐关" } });
			//game_str_setting.Add("waybill_font", new SettingStrItem() { desc = "露单字体", value = "大字体露单", values = new string[] { "大字体露单", "小字体露单" } });
			//game_str_setting.Add("break_machine", new SettingStrItem() { desc = "是否爆机", value = "爆机无", values = new string[] { "爆机无", "爆机有" } });
			//game_str_setting.Add("open_3_sec", new SettingStrItem() { desc = "3秒功能开", value = "3秒功能开", values = new string[] { "3秒功能开", "3秒功能关" } });
			//game_str_setting.Add("print_waybill", new SettingStrItem() { desc = "打印露单", value = "打印露单", values = new string[] { "打印露单" } });
			//game_str_setting.Add("single_double", new SettingStrItem() { desc = "单张牌", value = "单张牌", values = new string[] { "单张牌", "两张牌" } });
		}

		#region 按钮事件
		private void btnCurBillwayClick(object sender, RoutedEventArgs e)
		{
			var bu = sender as Button;
			Console.Write(bu.Tag);
		}

		private void btnPrintBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnTransmitBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnGeneralBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnImportBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnInsertBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnReplcBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnSaveBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnRegenerateBillway_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Homepage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Prepage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Jumpto_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnNextpage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnLastpage_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnImportFrontWaybill_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnDisconnect_Click(object sender, RoutedEventArgs e)
		{

		}
		#endregion
	}
}
