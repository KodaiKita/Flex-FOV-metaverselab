package net.id107.flexfov.projection;

import net.id107.flexfov.Reader;

public class MetaverseLab extends Projection {

    @Override
    public String getFragmentShader() {
        return Reader.read("flexfov:shaders/test.fsh");
    }

    @Override
    public float[] getBackgroundColor(boolean ignored) {
        return super.getBackgroundColor(skyBackground);
    }

    @Override
    public double getFovX() {
        return 360;
    }

    @Override
    public double getFovY() {
        return 180;
    }
}
