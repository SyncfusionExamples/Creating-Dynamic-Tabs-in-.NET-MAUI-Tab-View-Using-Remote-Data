# Creating-Dynamic-Tabs-in-.NET-MAUI-Tab-View-Using-Remote-Data

This sample demonstrates how to create dynamic tabs using a remote data source with the .NET MAUI Tab View control in a .NET MAUI application.

## Sample

```xaml
     <!--Gradient background-->
 <ContentPage.Background>
     <RadialGradientBrush Center="0.5,0.35" Radius="0.9">
         <GradientStop Color="#3A91F7" Offset="0.0" />
         <GradientStop Color="#0D2C4D" Offset="0.55" />
         <GradientStop Color="#071421" Offset="1.0" />
     </RadialGradientBrush>
 </ContentPage.Background>

 <VerticalStackLayout Padding="20" Spacing="20">

     <!--for selecting cities-->
     <tabview:SfTabView x:Name="LocationTabView" TabBarPlacement="Top" IndicatorBackground="White"
                    TabWidthMode="{OnPlatform Android=SizeToContent}" >

         <tabview:SfTabItem Header="Phoenix" TextColor="White" FontSize="16"/>
         <tabview:SfTabItem Header="Seattle" TextColor="White" FontSize="16"/>
         <tabview:SfTabItem Header="San Francisco" TextColor="White" FontSize="16"/>
         <tabview:SfTabItem Header="Miami" TextColor="White" FontSize="16"/>
         <tabview:SfTabItem Header="Denver" TextColor="White" FontSize="16"/>
         <tabview:SfTabItem Header="Chicago" TextColor="White" FontSize="16"/>
         <tabview:SfTabItem Header="New York" TextColor="White" FontSize="16"/>
     </tabview:SfTabView>

     <!-- Weather Display -->
     <ScrollView>
         <VerticalStackLayout Padding="20" Spacing="15">

             <!--current date-->
             <Label x:Name="DateLabel" FontSize="25" TextColor="White" HorizontalOptions="Center" />
             <!--current weather icon-->
             <Image x:Name="WeatherIcon" HeightRequest="100" HorizontalOptions="Center" />
             <!--current weather condition-->
             <Label x:Name="ConditionLabel" FontSize="28" TextColor="White" HorizontalOptions="Center" />
             <!--current temperature-->
             <Label x:Name="TempLabel" FontSize="46" TextColor="White" HorizontalOptions="Center" />

             <!-- Daily Forecast -->
             <ScrollView Orientation="Horizontal" HeightRequest="200">
                 <HorizontalStackLayout x:Name="NextDaysLayout"
                        Spacing="{OnPlatform Android=15,Default=55}"
                        HorizontalOptions="Center">

                 </HorizontalStackLayout>
             </ScrollView>

         </VerticalStackLayout>
     </ScrollView>

 </VerticalStackLayout>
```

## Requirements to run the demo

To run the demo, refer to [System Requirements for .NET MAUI](https://help.syncfusion.com/maui/system-requirements)

## Troubleshooting:

### Path too long exception

If you are facing path too long exception when building this example project, close Visual Studio and rename the repository to short and build the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion's samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's samples.