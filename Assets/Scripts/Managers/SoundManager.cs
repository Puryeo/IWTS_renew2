using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoManager<SoundManager>
{
    [System.Serializable]
    public class SoundEntry
    {
        public SoundId id;
        public AudioClip clip;

    }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private SoundEntry[] entries;

    private Dictionary<SoundId, AudioClip> _map;

    protected override void OnManagerAwake()
    {
        _map = new Dictionary<SoundId, AudioClip>(entries.Length);
        foreach (var e in entries)
        {
            if (e != null && e.clip != null) _map[e.id] = e.clip;
        }
    }


    // SFX ���(���� ���)
    public void PlaySFX(SoundId id)
    {
        if (sfxSource == null) return;

        if (_map.TryGetValue(id, out var clip) && clip != null)
        {
            sfxSource?.PlayOneShot(clip);
        }
    }

    // SFX ���(�ݺ� ���)
    public void StartLoopSFX(SoundId id)
    {
        Debug.Log("���1");
        if (sfxSource == null) return;

        if (_map.TryGetValue(id, out var clip) && clip != null)
        {
            Debug.Log("���2");
            // �̹� ���� Ŭ���� ���� ���̸� ����� ����
            if (sfxSource.loop && sfxSource.clip == clip && sfxSource.isPlaying) return;

            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
    }

    // SFX �ݺ� ��� ����
    public void StopLoopSFX()
    {
        if (sfxSource == null) return;

        if (sfxSource.loop)
        {
            sfxSource.Stop();
            sfxSource.loop = false;
            sfxSource.clip = null;
        }
    }

    // BGM ���
    public void PlayBGM()
    {
        // ���� �´� ������� �ν�����â���� �Ҵ� �� �ݺ� ��� �ǵ��� �ϱ�
    }


}
