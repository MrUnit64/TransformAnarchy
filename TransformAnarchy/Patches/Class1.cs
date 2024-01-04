using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TransformAnarchy
{
    /*internal class SizeFunctions
    {
        protected static Dictionary<string, float> buildableObjectCustomSizes = new Dictionary<string, float>();
        protected static Dictionary<string, float> buildableObjectParticleSystemSettingValues = new Dictionary<string, float>();

        public delegate void OnCustomSizeChangedHandler(BuildableObject buildableObject);
        public delegate void OnParticleSystemSettingValueChangedHandler(BuildableObject buildableObject);

        public static event Builder.OnCustomSizeChangedHandler OnCustomSizeChanged;
        public static event Builder.OnParticleSystemSettingValueChangedHandler OnParticleSystemSettingValueChanged;

        private void changeSize(float sizeDelta)
        {
            if (this.builtObjectGO == null)
            {
                return;
            }
            CustomSize customSize;
            if (this.builtObjectGO.TryGetComponent<CustomSize>(out customSize))
            {
                float num = customSize.getValue();
                float num2;
                if (Builder.tryGetStoredCustomSize(this.builtObjectGO, out num2))
                {
                    num = num2;
                }
                this.setSize(num + sizeDelta);
                return;
            }
            CustomParticleSystemSettings customParticleSystemSettings;
            if (this.builtObjectGO.TryGetComponent<CustomParticleSystemSettings>(out customParticleSystemSettings))
            {
                float num3 = customParticleSystemSettings.getValue();
                float num4;
                if (Builder.tryGetParticleSystemSettingValue(this.builtObjectGO, out num4))
                {
                    num3 = num4;
                }
                this.setSize(num3 + sizeDelta);
            }
        }

        public void setSize(float size)
        {
            if (this.builtObjectGO == null)
            {
                return;
            }
            CustomSize customSize;
            if (this.builtObjectGO.TryGetComponent<CustomSize>(out customSize))
            {
                float num = Mathf.Clamp(size, customSize.minSize, customSize.maxSize);
                Builder.storeCustomSize(this.builtObjectGO, num);
                GameObject gameObject = this.ghost;
                if (gameObject != null)
                {
                    CustomSize componentInChildren = gameObject.GetComponentInChildren<CustomSize>();
                    if (componentInChildren != null)
                    {
                        componentInChildren.setValue(num);
                    }
                }
                this.ghostChanged = true;
                CommandController.Instance.addCommand<ChangeCursorAttachmentSizeCommand>(new ChangeCursorAttachmentSizeCommand(size), null, true);
                return;
            }
            CustomParticleSystemSettings customParticleSystemSettings;
            if (this.builtObjectGO.TryGetComponent<CustomParticleSystemSettings>(out customParticleSystemSettings))
            {
                Mathf.Clamp(size, customParticleSystemSettings.multiplierMin, customParticleSystemSettings.multiplierMax);
                Builder.storeParticleSystemSettingValue(this.builtObjectGO, size);
                GameObject gameObject2 = this.ghost;
                if (gameObject2 != null)
                {
                    CustomParticleSystemSettings componentInChildren2 = gameObject2.GetComponentInChildren<CustomParticleSystemSettings>();
                    if (componentInChildren2 != null)
                    {
                        componentInChildren2.setValue(size);
                    }
                }
                this.ghostChanged = true;
            }
        }

        public static void storeCustomSize(BuildableObject buildableObject, float size)
        {
            Builder.buildableObjectCustomSizes[buildableObject.getReferenceName()] = size;
            Builder.OnCustomSizeChangedHandler onCustomSizeChanged = Builder.OnCustomSizeChanged;
            if (onCustomSizeChanged == null)
            {
                return;
            }
            onCustomSizeChanged(buildableObject);
        }

        public static void storeParticleSystemSettingValue(BuildableObject buildableObject, float value)
        {
            Builder.buildableObjectParticleSystemSettingValues[buildableObject.getReferenceName()] = value;
            Builder.OnParticleSystemSettingValueChangedHandler onParticleSystemSettingValueChanged = Builder.OnParticleSystemSettingValueChanged;
            if (onParticleSystemSettingValueChanged == null)
            {
                return;
            }
            onParticleSystemSettingValueChanged(buildableObject);
        }
    }*/
}
