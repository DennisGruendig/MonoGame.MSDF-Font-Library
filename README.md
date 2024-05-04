# MonoGame.MSDF_Font_Library
A font library which allows the MonoGame content manager to import font files directly, so that they can be rendered, using a [Multi-Channel Distance Fields](http://inter-illusion.com/assets/I2SmartEdgeManual/SmartEdge.html?MultiChannelDistanceFields.html) Shader.

The fonts are dynamically scalable with minimal quality losses while still being relatively light weight on the processor, since most of the calculations are Computed in a shader.

![Preview Screenshot](/Img/Screenshot_01.png?raw=true)

This project is based on the work of Viktor Chlumský found in his MSDF-Generator Projects, specifically [MSDF-Atlas-Gen](https://github.com/Chlumsky/msdf-atlas-gen).

Since this is still work in progress and it is actually my first public repository, I am thankful for any kind of productive feedback!

# Getting started

## Import Project Reference
At the current state, the project can only be used by including it in your solution. You need to reference it in your project so that you can use it's custom classes during runtime.

## Import Font Files
Build the project and reference the compiled `.dll` in your MonoGame Content Manager. Now you should be able to include `.ttf` and `.otf` files as regular content, which will then be handled by the content manager. As of now, the content processor will create temporary files, convert them to binary data and then delete them. If you set the parameter `"Keep Temporary"` to `true`, the files will not be deleted, so you can check them out for debugging.

During runtime, you can use the content manager to load the fonts into `FieldFont` classes.
```cs
FieldFont font = Content.Load<FieldFont>("<Your content name>");
```

## Import Shader
Drawing the fonts requires the file `MSDFShader.fx`, located in the [Shader](/MSDF%20Font%20Library/Shader) folder. Just include it in your content manager and make sure to load it during runtime.

## Batching Graphics
The final step to drawing fonts is the `FieldBatch` class. It's function is very similar to the built in `SpriteBatch` class, it needs to be constructed with `GraphicsDevice` and `MSDFShader`. Once constructed, you can use it inside your `Draw()` method to draw strings to the screen. Just make sure to call `Begin()` and `End()`, just like you would with a `SpriteBatch`.
