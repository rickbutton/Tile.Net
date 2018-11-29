var bar = context.Plugins.RegisterPlugin(new BarPlugin(new BarPluginConfig()
{
    BarTitle = "Workspacer.Bar",
    BarHeight = 50,
    FontSize = 16,
    DefaultWidgetForeground = Color.White,
    DefaultWidgetBackground = Color.Black,
    Background = Color.Black,
    LeftWidgets = () => new IBarWidget[] { new WorkspaceWidget(), new TextWidget(": "), new TitleWidget() },
    RightWidgets = () => new IBarWidget[] { new TimeWidget(), new ActiveLayoutWidget() },
}));