using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Games.MemoryGame.Scripts
{
    class OpieGameInterface : Opie.Game
    {
        public override string game_name()
        {
            return "MemoryGame";
        }

        public override void game_pause()
        {
            throw new NotImplementedException();
        }

        public override void game_resume()
        {
            throw new NotImplementedException();
        }

        public override void game_start()
        {
            //UnityEngine.SceneManagement.SceneManager.LoadScene("profiles_scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene("language_selection");
        }

        public override void game_stop()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("WebConnexionScene");
        }

        public override bool game_next()
        {
	return true;
        }

        public override bool game_previous()
        {
            return true;
        }
    }
}
