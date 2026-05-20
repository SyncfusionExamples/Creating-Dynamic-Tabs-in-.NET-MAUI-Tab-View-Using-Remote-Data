# Creating Dynamic Tabs in .NET MAUI Tab View Using Remote Data

This sample demonstrates how to create dynamic tabs using a remote data source with the .NET MAUI Tab View control in a .NET MAUI application.

## Sample

```xaml
<tabview:SfTabView x:Name="LocationTabView" ItemsSource="{Binding Cities}" IndicatorBackground="White">

    <!-- Header: city title -->
    <tabview:SfTabView.HeaderItemTemplate>
        <DataTemplate>
            <Label Text="{Binding Name}" TextColor="White" VerticalTextAlignment="Center"/>
        </DataTemplate>
    </tabview:SfTabView.HeaderItemTemplate>

    <!-- Content: weather UI -->
    <tabview:SfTabView.ContentItemTemplate>
        <DataTemplate>
            <ScrollView>
                <VerticalStackLayout>

                    <!-- current date -->
                    <Label Text="{Binding DateText}" TextColor="White" HorizontalOptions="Center" />

                    <!-- current weather icon -->
                    <Image Source="{Binding Icon}" HorizontalOptions="Center"/>

                    <!-- current weather condition -->
                    <Label Text="{Binding ConditionText}" HorizontalOptions="Center" TextColor="White"/>

                    <!-- current temperature -->
                    <Label Text="{Binding TempText}" TextColor="White" HorizontalOptions="Center" />

                    <!-- Daily Forecast -->
                    <ScrollView Orientation="Horizontal">
                        <CollectionView ItemsSource="{Binding NextDays}" ItemsLayout="HorizontalList"
                                        HorizontalOptions="Center" SelectionMode="None">

                            <!-- Scrollable view for daily weather forecast -->

                        </CollectionView>
                    </ScrollView>
                </VerticalStackLayout>
            </ScrollView>
        </DataTemplate>
    </tabview:SfTabView.ContentItemTemplate>
</tabview:SfTabView>
```

### Output

![Tab View](AndroidEmulator-nexus-5-api-35-52-ezgif.com-resize.gif)

## Requirements to run the demo

To run the demo, refer to [System Requirements for .NET MAUI](https://help.syncfusion.com/maui/system-requirements)

## Troubleshooting:

### Path too long exception

If you are facing path too long exception when building this example project, close Visual Studio and rename the repository to short and build the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion's samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's samples.

Refer to the blog for step-by-step guidance on [Building a Real-Time Weather Dashboard with .NET MAUI Tab View](https://www.syncfusion.com/blogs/post/real-time-weather-dashboard-maui-tab-view).