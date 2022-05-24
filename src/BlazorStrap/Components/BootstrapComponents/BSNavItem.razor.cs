using BlazorComponentUtilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStrap
{
    public partial class BSNavItem : BlazorStrapBase, IDisposable
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        /// <summary>
        /// Data-Blazorstrap attribute value to target.
        /// </summary>
        [Parameter] public string? Target { get; set; }

        /// <summary>
        /// Sets if the NavItem is active.
        /// </summary>
        [Parameter] public bool? IsActive { get; set; }

        /// <summary>
        /// Sets if the NavItem is disabled.
        /// </summary>
        [Parameter] public bool IsDisabled { get; set; }

        /// <summary>
        /// Sets if the NavItem is a dropdown.
        /// </summary>
        [Parameter] public bool IsDropdown { get; set; }

        /// <summary>
        /// Removes the <c>nav-item</c> class.
        /// </summary>
        [Parameter] public bool NoNavItem { get; set; }

        /// <summary>
        /// Display nav item as active if a child route of the nav item is active.
        /// </summary>
        [Parameter] public bool ActiveOnChildRoutes { get; set; } = false;

        /// <summary>
        /// CSS class to apply to the nav bar list items.
        /// </summary>
        [Parameter] public string? ListItemClass { get; set; }

        /// <summary>
        /// Event called when item is clicked.
        /// </summary>
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        /// <summary>
        /// Prevent default on click behavior.
        /// </summary>
        [Parameter] public bool PreventDefault { get; set; }

        /// <summary>
        /// Content of tab.
        /// </summary>
        [Parameter] public RenderFragment? TabContent { get; set; }

        /// <summary>
        /// Tab label.
        /// </summary>
        [Parameter] public RenderFragment? TabLabel { get; set; }

        /// <summary>
        /// Url for nav link.
        /// </summary>
        [Parameter] public string? Url { get; set; } = "javascript:void(0);";

        [CascadingParameter] public BSNav? Parent { get; set; }
        private bool _canHandleActive;
        private string? ClassBuilder => new CssBuilder("nav-link")
            .AddClass("active", IsActive ?? false)
            .AddClass("disabled", IsDisabled)
            .AddClass(LayoutClass, !string.IsNullOrEmpty(LayoutClass))
            .AddClass(Class, !string.IsNullOrEmpty(Class))
            .Build().ToNullString();

        private string? ListClassBuilder => new CssBuilder()
            .AddClass("nav-item", !NoNavItem)
            .AddClass("dropdown", IsDropdown)
            .AddClass(ListItemClass)
            .Build().ToNullString();

        protected override void OnInitialized()
        {
            if (IsActive == null)
            {
                _canHandleActive = true;
                if (NavigationManager.Uri == NavigationManager.BaseUri + Url?.TrimStart('/'))
                    IsActive = true;
                if (NavigationManager.Uri.Contains(NavigationManager.BaseUri + Url?.TrimStart('/')) && ActiveOnChildRoutes)
                    IsActive = true;
                NavigationManager.LocationChanged += OnLocationChanged;
            }
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            if (!_canHandleActive) return;
            if (Parent?.IsTabs ?? false) return;
            IsActive = false;
            if (NavigationManager.Uri == NavigationManager.BaseUri + Url?.TrimStart('/'))
                IsActive = true;
            if (NavigationManager.Uri.Contains(NavigationManager.BaseUri + Url?.TrimStart('/')) && ActiveOnChildRoutes)
                IsActive = true;
            StateHasChanged();
        }

        protected override void OnParametersSet()
        {
            if (Parent == null) return;
            if (Parent.IsTabs)
            {
                IsActive = Parent.SetFirstChild(this);
            }
            Parent.ChildHandler += Parent_ChildHandler;
        }

        private async Task ClickEvent()
        {
            if (!string.IsNullOrEmpty(Target))
                BlazorStrap.ForwardClick(Target);

            if (OnClick.HasDelegate)
                await OnClick.InvokeAsync();
            if (Parent?.IsTabs ?? false)
            {
                Parent.Invoke(this);
            }
        }

        private async void Parent_ChildHandler(BSNavItem sender)
        {
            if (Parent != null)
                IsActive = Parent.ActiveChild == this;
            await InvokeAsync(StateHasChanged);
        }
        public void Dispose()
        {
            if (_canHandleActive)
                NavigationManager.LocationChanged -= OnLocationChanged;
            if (Parent == null) return;
            if (Parent.ActiveChild == this)
                Parent.ActiveChild = null;
            Parent.ChildHandler -= Parent_ChildHandler;
        }
    }
}