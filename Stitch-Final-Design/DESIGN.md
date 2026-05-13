---
name: H2BIG Design System
colors:
  surface: '#fbf9f8'
  surface-dim: '#dcd9d9'
  surface-bright: '#fbf9f8'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f6f3f2'
  surface-container: '#f0eded'
  surface-container-high: '#eae8e7'
  surface-container-highest: '#e4e2e1'
  on-surface: '#1b1c1c'
  on-surface-variant: '#424752'
  inverse-surface: '#303030'
  inverse-on-surface: '#f3f0f0'
  outline: '#727783'
  outline-variant: '#c2c6d4'
  surface-tint: '#005db5'
  primary: '#00488d'
  on-primary: '#ffffff'
  primary-container: '#005fb8'
  on-primary-container: '#cadcff'
  inverse-primary: '#a8c8ff'
  secondary: '#5d5f5f'
  on-secondary: '#ffffff'
  secondary-container: '#dfe0e0'
  on-secondary-container: '#616363'
  tertiary: '#424950'
  on-tertiary: '#ffffff'
  tertiary-container: '#596167'
  on-tertiary-container: '#d5dce4'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d6e3ff'
  primary-fixed-dim: '#a8c8ff'
  on-primary-fixed: '#001b3d'
  on-primary-fixed-variant: '#00468b'
  secondary-fixed: '#e2e2e2'
  secondary-fixed-dim: '#c6c6c7'
  on-secondary-fixed: '#1a1c1c'
  on-secondary-fixed-variant: '#454747'
  tertiary-fixed: '#dce3eb'
  tertiary-fixed-dim: '#c0c7cf'
  on-tertiary-fixed: '#151c22'
  on-tertiary-fixed-variant: '#40484e'
  background: '#fbf9f8'
  on-background: '#1b1c1c'
  surface-variant: '#e4e2e1'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 30px
    fontWeight: '600'
    lineHeight: 38px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-base:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  label-bold:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
  code-data:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '500'
    lineHeight: 18px
    letterSpacing: 0.01em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 4px
  container-padding: 24px
  gutter: 16px
  stack-sm: 8px
  stack-md: 16px
  stack-lg: 32px
---

## Brand & Style
The design system is engineered for the precision-critical environment of advanced water management. It draws inspiration from the functional heritage of Windows Forms—prioritizing density, discoverability, and utility—while modernizing the aesthetic for high-performance web and desktop environments. 

The style is **Corporate & Functional**, characterized by a rigorous commitment to visual hierarchy and data legibility. It evokes a sense of reliability and institutional trust, ensuring that operators can manage complex hydraulic data without visual fatigue. The aesthetic avoids unnecessary ornamentation, focusing instead on crisp borders, clear alignment, and high-contrast interactions.

## Colors
The palette is dominated by **Hydration Blue (#005FB8)**, used strategically to denote primary actions and system-level branding. The interface utilizes a **Clean White (#FFFFFF)** for high-priority surfaces and data containers, providing a stark contrast against the **Slate Gray (#333333)** typography. 

The background employs a **soft Light Blue (#F0F7FF)** to reduce glare and differentiate the application frame from content modules. Semantic colors (Success, Warning, Danger) are aligned with standard industrial conventions to ensure immediate recognition of system alerts and water quality status.

## Typography
This design system utilizes **Inter** for its exceptional legibility and systematic structure, serving as a modern successor to traditional Windows system fonts. 

Typography is optimized for information density. Body text is set at 14px for standard interfaces, with a 13px variant reserved for complex data grids and technical logs. High contrast is maintained throughout, using font weight rather than color shifts to establish hierarchy, ensuring that critical readings remain legible under varying lighting conditions in control rooms.

## Layout & Spacing
The layout employs a **Fluid Grid** model designed to maximize screen real estate for data visualization. A sidebar-driven navigation provides persistent access to system modules, while the main content area utilizes a flexible 12-column system.

Spacing follows a strict 4px baseline grid to maintain the "compact" feel characteristic of professional management tools. Elements are packed tightly to reduce scrolling, but white space is used deliberately between functional groups to prevent cognitive overload. Margins are consistent across views to ground the user in a stable, predictable environment.

## Elevation & Depth
In alignment with the functional Windows Forms aesthetic, the design system utilizes **Bold Borders and Tonal Layers** rather than soft shadows to convey depth. 

Surface levels are defined by color:
- **Level 0 (Background):** Light Blue (#F0F7FF) for the application shell.
- **Level 1 (Cards/Grids):** Pure White (#FFFFFF) with a 1px Slate Gray or Hydration Blue border.
- **Level 2 (Modals/Popovers):** Pure White with a subtle 2px blur shadow to indicate priority over the main workspace.

Interactive elements use "inset" border styles for active states to mimic physical button presses, reinforcing the tactile, tool-like nature of the interface.

## Shapes
The shape language is primarily **Soft (0.25rem)**. This slight rounding provides a modern touch to the otherwise rigid, grid-based layout without sacrificing the professional, "engineered" feel of the system. 

Secondary elements like data chips use the same 4px radius, while primary search bars and buttons maintain this consistency. Form inputs and data grid cells remain sharp-cornered or minimally rounded to maximize the usable internal area for text and numbers.

## Components
### Action Buttons
Buttons feature a high-contrast fill for primary actions (Hydration Blue with White text) and a ghost-style border for secondary actions. State changes (hover/active) should be immediate and distinct, utilizing darker shades of blue to indicate engagement.

### Data Grids
The centerpiece of the system. Grids use alternating row stripes in the background color for scanning legibility. Headers are "sticky" with a Slate Gray font and a persistent bottom border. Columns are resizable, and cell padding is kept to a functional minimum (8px) to maximize data density.

### Search Bars
Search bars are designed as "Global Filters," featuring a Clean White background, a Slate Gray search icon, and a persistent 1px border. They should occupy the top-right of data-heavy sections for quick access.

### Form Inputs
Inputs utilize a 1px border that thickens and changes to Hydration Blue on focus. Error states are indicated by a 2px red border and a small inline warning icon, ensuring errors are caught immediately during data entry.

### Status Chips
Used for water quality and system health indicators. These are small, non-interactive badges with high-contrast text and a subtle background tint of the status color (e.g., light green background with dark green text for "Optimal").