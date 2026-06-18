
namespace CollabXR.ModExtras
{
    /** <summary>
     * Component interface used as data encapsulation for different playback sets (like different animation that can be
     * changed through a drop-down in the ui, for example). Anything that needs flexibility like this should set up
     * a PlaybackComponent.
     * </summary> */
    public interface IPlaybackComponent
    {
	    /**
	     * <summary>
	     * The name to be displayed in the UI for this component.
	     * </summary>
	     */
	    public string DisplayName { get; }

	    /**
	     * <summary>
	     * The name to display for each set in the UI.
	     * </summary>
	     */
	    public string GetSetDisplayName(int index);

	    /**
	     * <summary>
	     * The currently active index.
	     * </summary>
	     */
	    public int ActiveSetIndex { get; set; }

	    /**
	     * <summary>
	     * The number of total sets available to swap between.
	     * </summary>
	     */
	    public int SetCount { get; set; }

	    /**
	     * <summary>
	     * The ID of this specific component type. Because components are unique in ECS, we use this as a
	     * simple way to transfer data across the network using a map. This index should probably be
	     * generated using a UUID but I dont really know how to do that so I will just be hardcoding it to
	     * a unique number.
	     * </summary>
	     */
	    int ComponentId { get; }

	    /**
	     * <summary>
	     * Functionality for updating the active set, which changes the internally stored value and also
	     * updates any dependencies as necessary.
	     * </summary>
	     */
	    void SetComponentValues(int index);
    }
}

