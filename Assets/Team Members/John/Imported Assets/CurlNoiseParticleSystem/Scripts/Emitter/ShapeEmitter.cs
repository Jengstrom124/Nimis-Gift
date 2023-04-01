using System.Collections;
using UnityEngine;

namespace CurlNoiseParticleSystem.Emitter
{
    /// <summary>
    /// Emit particle from shape surface.
    /// </summary>
    public class ShapeEmitter : MonoBehaviour
    {
        public static ShapeEmitter instance;

        [SerializeField]
        private MeshFilter _filter;

        [SerializeField]
        private int _countPerParticle = 1;

        [SerializeField]
        private float _delay = 0.5f;

        private CurlParticle _particle;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            _particle = CurlParticleSystem.Instance.Get();
            _particle.AutoRelease = false;
        }

        /// <summary>
        /// Burst with particle param list.
        /// </summary>
        public void Emit(float delay)
        {
            StartCoroutine(EmitCoroutine(delay));
        }
        IEnumerator EmitCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            CurlParticle particle = CurlParticleSystem.Instance.Get();
            particle.EmitWithMesh(_filter, _countPerParticle, _delay);
        }
    }
}
