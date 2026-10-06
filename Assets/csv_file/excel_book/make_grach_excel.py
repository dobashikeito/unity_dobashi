# brain用に作成、グラフ生成キット
import pandas as pd
from datetime import datetime
import openpyxl as px
from openpyxl.chart import ScatterChart, Reference, Series
from openpyxl.chart.axis import DateAxis

#file名を入力させ、保存
count=0
time_row_start=[]#timeの最初の行番号
time_row_end=[]#timeの最後の行番号
time_end_value=[]#時間を保存、行はtime_row_end
success_rate=[]#成功率
count_touch_value=[]#接触回数を保存、行はtime_row_end+1,列は４
count_over_shock=[]#ある加速度を超えた回数を保存、行はtime_row_end+1,列は8
title_list=["ピッチ角の時間遷移","ロール角の時間遷移","経路y座標","経路x座標","経路z座標","y方向の加速度"]
y_axis_list=["pitch(deg)","roll(deg)","y(m)","x(m)","z(m)","y(m/s2)"]
y_axis_min=[-90,-60,0,-1,-10,-15]
y_axis_max=[90,60,0.7,2,1,15]
y_axis_interval=[10,10,0.2,0.2,0.5,1]
sell_width=["G","P","Y","AH","AQ","AZ","BI","BR","CA","CJ","CS","DB","DK","DT","EC","EL","EU","FD","FM","FV","GE","GN","GW","HF","HO","IG","IP","IY","JH","JQ","JZ","KI","KR","LA","LJ","LS","MB","MK","MT","NC","NL","NU","OD","OM","OV","PE","PN","PW","QF","QO","QX","RG","RP","RY","SH","SQ","SZ","TI","TR","UA","UJ","US","VB","VK","VT","WC","WL","WU","XD"]

filepath=input("file名は何ですか→")

excel_file=px.load_workbook(filepath)
# excel_sheet=excel_file['time1']
excel_sheet=excel_file.active

for i in range(1,1000000):
    if excel_sheet.cell(row=i,column=1).value=="time(s)":
        time_row_start.append(i)
        count=0
    if excel_sheet.cell(row=i,column=1).value==None:
        count=count+1
    if count==1:
        time_row_end.append(i)
        if excel_sheet.cell(row=i,column=5).value=="fault":
            time_end_value.append(0)
            success_rate.append(0)
        else:
            time_end_value.append(excel_sheet.cell(row=i-1,column=1).value)
            success_rate.append(1)
        count_touch_value.append(excel_sheet.cell(row=i,column=4).value)
        count_over_shock.append(excel_sheet.cell(row=i,column=8).value)
    if count==2:
        break

for i in range(0,len(time_row_start)):
    for k in range(0,len(title_list)):
        # chart = px.chart.LineChart()
        chart = ScatterChart()
        # y軸のデータ
        data=px.chart.Reference(excel_sheet,k+2,time_row_start[i],k+2,time_row_end[i]-1)#左上列、行、右下列行
        # x軸のデータ
        categories=px.chart.Reference(excel_sheet,1,time_row_start[i]+1,1,time_row_end[i]-1)#左上列、行、右下列行
        series=Series(data,categories,title_from_data=True)
        chart.series.append(series)
        # chart.add_data(data,titles_from_data=True)
        # chart.set_categories(categories)
        chart.style = 3
        num=str(i+1)
        chart.title = title_list[k]+"（"+num+"回目）"
        chart.y_axis.title = y_axis_list[k]
        chart.x_axis.title = "時間(s)"
        chart.height = 9
        chart.width = 16
        chart.x_axis.scaling.min = 0
        chart.x_axis.scaling.max = 100
        chart.y_axis.scaling.min = y_axis_min[k]
        chart.y_axis.scaling.max = y_axis_max[k]
        chart.x_axis.majorUnit = 10
        chart.y_axis.majorUnit = y_axis_interval[k]

        excel_sheet.add_chart(chart,""+sell_width[i]+str(k*15+2)+"")
        if k==len(title_list)-1:
            chart = ScatterChart()
            # y軸のデータ
            data=px.chart.Reference(excel_sheet,5,time_row_start[i],5,time_row_end[i]-1)#左上列、行、右下列行
            # x軸のデータ
            categories=px.chart.Reference(excel_sheet,6,time_row_start[i]+1,6,time_row_end[i]-1)#左上列、行、右下列行
            series=Series(data,categories,title_from_data=True)
            chart.series.append(series)
            # chart.add_data(data,titles_from_data=True)
            # chart.set_categories(categories)
            chart.style = 3
            num=str(i+1)
            chart.title = "経路（"+num+"回目）"
            chart.y_axis.title = "x(m)"
            chart.x_axis.title = "z(m)"
            chart.height = 9
            chart.width = 16
            chart.x_axis.scaling.min = -10
            chart.x_axis.scaling.max = 2
            chart.y_axis.scaling.min = -1
            chart.y_axis.scaling.max = 2
            chart.x_axis.majorUnit = 1
            chart.y_axis.majorUnit = 0.5
            excel_sheet.add_chart(chart,""+sell_width[i]+str(k*15+17)+"")

excel_sheet["J1"]="times"
excel_sheet["K1"]="goal_time"
excel_sheet["L1"]="count_touch"
success_rate_value=sum(success_rate)/len(success_rate)
excel_sheet["M1"]="success rate "
excel_sheet["M2"]=str(success_rate_value)
excel_sheet["N1"]="success"
excel_sheet["N2"]=str(sum(success_rate))
excel_sheet["O1"]="count_over_time"

for i in range(2,len(time_end_value)+2):
    excel_sheet["J"+str(i)]=i-1
    excel_sheet["K"+str(i)]=time_end_value[i-2]
    excel_sheet["L"+str(i)]=count_touch_value[i-2]
    excel_sheet["O"+str(i)]=count_over_shock[i-2]

chart = ScatterChart()
# y軸
data=px.chart.Reference(excel_sheet,11,1,11,11)#左上列、行、右下列行
# x軸
categories=px.chart.Reference(excel_sheet,10,1,10,11)#左上列、行、右下列行
series=Series(data,categories,title_from_data=True)
chart.series.append(series)
chart.style = 3
chart.title = "ゴール時間"
chart.y_axis.title = "goal time(s)"
chart.x_axis.title = "times"
chart.height = 9
chart.width = 16
chart.x_axis.scaling.min = 0
chart.x_axis.scaling.max = 11
chart.y_axis.scaling.min = 0
chart.y_axis.scaling.max = 100
chart.x_axis.majorUnit = 1
chart.y_axis.majorUnit = 10
excel_sheet.add_chart(chart,"A2")

chart1 = ScatterChart()
# y軸
data1=px.chart.Reference(excel_sheet,12,1,12,11)#左上列、行、右下列行
# x軸
categories1=px.chart.Reference(excel_sheet,10,1,10,11)#左上列、行、右下列行
series1=Series(data1,categories1,title_from_data=True)
chart1.series.append(series1)
chart1.style = 3
chart1.title = "接触回数"
chart1.y_axis.title = "touch times"
chart1.x_axis.title = "times"
chart1.height = 9
chart1.width = 16
chart1.x_axis.scaling.min = 0
chart1.x_axis.scaling.max = 11
chart1.y_axis.scaling.min = 0
chart1.y_axis.scaling.max = 100
chart1.x_axis.majorUnit = 1
chart1.y_axis.majorUnit = 10
excel_sheet.add_chart(chart1,"A17")

chart2 = ScatterChart()
# y軸
data2=px.chart.Reference(excel_sheet,15,1,15,11)#左上列、行、右下列行
# x軸
categories2=px.chart.Reference(excel_sheet,10,1,10,11)#左上列、行、右下列行
series2=Series(data2,categories2,title_from_data=True)
chart2.series.append(series2)
chart2.style = 3
chart2.title = "衝撃回数"
chart2.y_axis.title = "shock over time"
chart2.x_axis.title = "times"
chart2.height = 9
chart2.width = 16
chart2.x_axis.scaling.min = 0
chart2.x_axis.scaling.max = 11
chart2.y_axis.scaling.min = 0
chart2.y_axis.scaling.max = 100
chart2.x_axis.majorUnit = 1
chart2.y_axis.majorUnit = 10
excel_sheet.add_chart(chart2,"A32")

excel_file.save("gragh_"+filepath)