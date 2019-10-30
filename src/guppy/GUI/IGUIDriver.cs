using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Guppy2.GFX;

namespace Guppy2.GUI
{

    public enum DirectoryType {
        Application,
        LocalizedMessages
    }

    public enum PictureMode
    {
        Hardware,
        Software
    }

    public interface IGUIDriver
    {
        bool Open();
        void Run();
        void Quit();

        Size2i ScreenResolution { get; }
        Window ForegroundWindow { get; }
        void Wait(bool blocking);


        //Control creation
        IDriverButton CreateButton(GUIObject owner,string caption);
        IDriverWindow CreateWindow(GUIObject owner,string caption,WindowStyle style);
        IDriverPanel CreatePanel(GUIObject owner);
        IDriverFrame CreateFrame(GUIObject owner,string caption);
        IDriverToggle CreateToggle(GUIObject owner, string caption);
        IDriverEdit CreateEdit(GUIObject owner);
        IDriverLabel CreateLabel(GUIObject owner,string caption);
        IDriverSeparator CreateSeparator(GUIObject owner, bool verical);
        IDriverListBox CreateListBox(GUIObject owner);
        IDriverChoice CreateChoice(GUIObject owner);
        IDriverEditChoice CreateEditChoice(GUIObject owner);
        IDriverProgressBar CreateProgressBar(GUIObject owner); //TODO: support vertical?
        IDriverSlider CreateSlider(GUIObject owner, bool vertical);
        IDriverTabs CreateTabs(GUIObject owner);
        IDriverTabPage CreateTabPage(GUIObject owner_tabs,string caption);
        IDriverSplitter CreateSplitter(GUIObject owner,bool vertical);
        IDriverCanvas CreateCanvas(GUIObject owner);
        
        IDriverTypeface CreateTypeface(string name, double size, TypefaceStyle style);

        Picture CreatePicture(Stream source, PictureMode mode);
        Picture CreatePicture(int w, int h, PictureMode mode);



        
    }


    /// <summary>
    /// Base interface for all GUI object, widgets, images etc.
    /// </summary>
    public interface IDriverGUIObject:IDisposable
    {
        object NativeObject { get; }
    }

    public interface IDriverWidget:IDriverGUIObject
    {
        Size2i NaturalSize { get; }
        Size2i PhysicalSize { get; }
        void Show();
        void Hide();
        void Redraw();
        void RedrawLater();
        Rect2i Bounds { get; set; }
        Group Parent { get; }
        Typeface Typeface { set; }
        bool Focused { get; set; }
        string Tip { get; set; }
        bool Enabled { get; set; }
    }
    
    public interface IDriverGroup : IDriverWidget
    {
        void Append(IDriverWidget child);
        void Detach(IDriverWidget child);
        Margin BorderDecorMargin { get; }
        Point2i Origin { get; }
    }

    public interface IDriverWindow : IDriverGroup
    {
        string Caption { get; set; }
        Size2i MinSize { get; set; }
        void ShowModal();
        bool AutoDispose { get; set; }
        Button AcceptButton { get; set; }
        void Close();
        Widget ActiveChild { get; }

    }
    
    public interface IDriverPanel : IDriverGroup
    {
        
    }


    public interface IDriverSplitter : IDriverGroup
    {
        SplitterPanel Panel1 { get; }
        SplitterPanel Panel2 { get; }
        bool Vertical { get; set; }
        int SplitterWidth { get; set; }
        int SplitterPosition { get; set; }

    }
    
    public interface IDriverSplitterPanel : IDriverGroup
    {

    }

    public interface IDriverFrame : IDriverGroup
    {

    }

    public interface IDriverButton : IDriverWidget
    {
        string Caption { get; set; }
        Picture Picture { set; }
        bool Flat { get; set; }
        bool CanFocus { get; set; }
    }

    public interface IDriverToggle : IDriverWidget
    {
        string Caption { get; set; }
        bool Checked { get; set; }
    }


    public interface IDriverEdit:IDriverWidget
    {
        string Text { get; set; }
    }

    public interface IDriverLabel : IDriverWidget
    {
        string Caption { get; set; }
    }

    public interface IDriverSeparator : IDriverWidget
    {
        
    }
    
    public interface IDriverTypeface:IDriverGUIObject
    {
        
    }

    public interface IDriverListBox : IDriverWidget
    {
        int Add(object obj,Picture pic);
        void Insert(int index, object obj,Picture pic);
        void Remove(int index);
        void Clear();
        int SelectedIndex { get; set; }
        bool Sorted { get; set; }
        int Count { get; }
        bool MultiSelect { get; set; }
        void SetSelected(int index, bool selected);
        bool GetSelected(int index);
        int TopItem { set; }
        void SetPicture(int index, Picture image);
        void SetObject(int index, object data);
        object this[int idx]{get;}
    }

    public interface IDriverChoice : IDriverWidget
    {
        int Add(object obj, Picture pic);
    }

    public interface IDriverEditChoice : IDriverWidget
    {
        int Add(object obj, Picture pic);
    }
    
    public interface IDriverProgressBar : IDriverWidget
    {
        double Value { get; set; } //always in range 0.0-1.0 in driver
    }

    public interface IDriverSlider : IDriverWidget
    {
        double Value { get; set; } //always in range 0.0-1.0 in driver
    }


    public interface IDriverTabs : IDriverGroup
    {
        TabPage SelectedPage { get; set; }
        TabSide TabSide { get; set; }
    }

    public interface IDriverTabPage : IDriverGroup
    {
        string Caption {get;set;}
    }


    public interface IDriverImage : IDriverGUIObject
    {
        int Width { get; }
        int Height { get; }
    }

    public interface IDriverCanvas : IDriverGUIObject
    {
        Guppy2.GFX.Painter CreatePainter();
    }
}
