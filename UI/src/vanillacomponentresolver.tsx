import { Color, Theme, UniqueFocusKey } from "cs2/bindings";
import { InputAction } from "cs2/input";
import { ModuleRegistry } from "cs2/modding";
import { BalloonDirection, FocusKey, PanelTheme } from "cs2/ui";
import { CSSProperties, HTMLAttributes, ReactNode } from "react";

// These are specific to the types of components that this mod uses.
// In the UI developer tools at http://localhost:9444/ go to Sources -> Index.js. Pretty print if it is formatted in a single line.
// Search for the tsx or scss files. Look at the function referenced and then find the properies for the component you're interested in.
// As far as I know the types of properties are just guessed.
type PropsToolButton = {
    focusKey?: UniqueFocusKey | null
    src: string
    selected : boolean
    multiSelect : boolean
    disabled?: boolean
    tooltip?: string | null
    selectSound?: any
    uiTag?: string
    className?: string
    children?: string | JSX.Element | JSX.Element[]
    onSelect?: (x: any) => any,
} & HTMLAttributes<any>

type PropsSection = {
    title?: string | null
    uiTag?: string
    children: string | JSX.Element | JSX.Element[]
}

type ToggleProps = {
    focusKey?: FocusKey;
    checked?: boolean;
    disabled?: boolean;
    style?: CSSProperties;
    className?: string;
    children?: ReactNode;
    onChange?: () => void;
    onMouseOver?: () => void;
    onMouseLeave?: () => void;
}

type Checkbox = {
    checked?: boolean;
    disabled?: boolean;
    className?: string;
    theme?: any;
} & HTMLAttributes<any>;

// var C4 = function() {

// Use of it 
/** From the transport-line-item.tsx
 *  z.jsx)("div", {
        className: Zve.cellSingle,
        children: (0,
        z.jsx)(Tp, {
            tooltip: (0,
            z.jsx)(Zc.Transport.TOOLTIP_COLOR, {
                hash: x
            }),
            children: (0,
            z.jsx)(C4, {
                value: n.lineData.color,
                className: nbe.colorField,
                onChange: f,
                onClick: lv
            })
        })
    })
 */
type ColorField = {
    focusKey?: FocusKey;
    disabled?: boolean;
    value?: Color;
    className?: string;
    selectAction?: InputAction;
    alpha?: any;
    popupDirection?: BalloonDirection;
    onChange?: (e: Color) => void;
    onClick?: (e: any) => void;
    onMouseEnter?: (e: any) => void;
    onMouseLeave?: (e: any) => void;
}

// function P4(e) {
// type BoundColorField = {
//     value?: any;    
//     disabled?: boolean;    
// }

// This is an array of the different components and sass themes that are appropriate for your UI. You need to figure out which ones you need from the registry.
const registryIndex = {
    Section: ["game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", "Section"],
    ToolButton: ["game-ui/game/components/tool-options/tool-button/tool-button.tsx", "ToolButton"],
    toolButtonTheme: ["game-ui/game/components/tool-options/tool-button/tool-button.module.scss", "classes"],
    Toggle: ["game-ui/common/input/toggle/toggle.tsx", "Toggle"],
    toggleTheme: ["game-ui/menu/widgets/toggle-field/toggle-field.module.scss", "classes"],
    Checkbox: ["game-ui/common/input/toggle/checkbox/checkbox.tsx", "Checkbox"],    
    checkboxTheme: ["game-ui/common/input/toggle/checkbox/checkbox.module.scss", 'classes'],
    ColorField: ["game-ui/common/input/color-picker/color-field/color-field.tsx", 'ColorField'],
    BoundColorField: ["game-ui/common/input/color-picker/color-field/color-field.tsx", 'BoundColorField']    
}

export class VanillaComponentResolver {
    // As far as I know you should not need to edit this portion here. 
    // This was written by Klyte for his mod's UI but I didn't have to make any edits to it at all. 
    public static get instance(): VanillaComponentResolver { return this._instance!! }
    private static _instance?: VanillaComponentResolver

    public static setRegistry(in_registry: ModuleRegistry) { this._instance = new VanillaComponentResolver(in_registry); }
    private registryData: ModuleRegistry;

    constructor(in_registry: ModuleRegistry) {
        this.registryData = in_registry;
    }

    private cachedData: Partial<Record<keyof typeof registryIndex, any>> = {}
    private updateCache(entry: keyof typeof registryIndex) {
        const entryData = registryIndex[entry]; 
        return this.cachedData[entry] = this.registryData.registry.get(entryData[0])!![entryData[1]]
    }

    // This section defines your components and themes in a way that you can access via the singleton in your components.
    // Replace the names, props, and strings as needed for your mod.
    public get Section(): (props: PropsSection) => JSX.Element { return this.cachedData["Section"] ?? this.updateCache("Section") }
    public get ToolButton(): (props: PropsToolButton) => JSX.Element { return this.cachedData["ToolButton"] ?? this.updateCache("ToolButton") }
    public get Toggle(): (props: ToggleProps) => JSX.Element { return this.cachedData["Toggle"] ?? this.updateCache("Toggle") }
    public get Checkbox(): (props: Checkbox) => JSX.Element { return this.cachedData["Checkbox"] ?? this.updateCache("Checkbox") }
    public get ColorField(): (props: ColorField) => JSX.Element { return this.cachedData["ColorField"] ?? this.updateCache("ColorField") }

    public get toggleTheme(): Theme | any { return this.cachedData["toggleTheme"] ?? this.updateCache("toggleTheme") }
    public get checkboxTheme(): Theme | any { return this.cachedData["checkboxTheme"] ?? this.updateCache("checkboxTheme") }
    public get toolButtonTheme(): Theme | any { return this.cachedData["toolButtonTheme"] ?? this.updateCache("toolButtonTheme") }

} 