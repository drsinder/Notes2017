/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: CrazyWordsController.cs
**
**  Description:
**      Crazy Words Controller for Notes 2017
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/


using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2017.Models;
using Notes2017.Services;
using Notes2017.Data;
using Microsoft.EntityFrameworkCore;

namespace Notes2017.Controllers
{
    [Authorize(Roles = "CrazyWords")]
    public class CrazyWordsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CrazyWordsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ApplicationDbContext applicationDbContext)
        {
            _db = applicationDbContext;
        }

        // list of four sets of letters to work with
        private readonly string[] _goodList = new string[]{ "ALGMEBCDITOHFNR", "ALGMEBCDIKOHFNR", "ALGMEBCDUTOHFNR", "AKYMEBCDUKOHFNR" };

        /// <summary>
        /// Show game
        /// </summary>
        /// <param name="id">word list index to use 0..3</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int id)
        {
            return View(await SetTest(id, false));
        }

        /// <summary>
        /// Logic for game
        /// </summary>
        /// <param name="model">Object containing game paramters and user input</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ViewModels.CrazyWords.NewWordViewModel model)
        {
            int wordList = model.wordList;

            bool bymostrecent = model.MostRecentFirst;

            // get the word to test and the user name
            string test = model.myWord.Word.ToUpper().Trim();
            string user = User.Identity.Name;

            ViewData["Message"] = " ";
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = test + " is not four letters!";
                return View(await SetTest(wordList, bymostrecent));
            }

            if (test.Length != 4)
            {
                ViewData["Message"] = test + " is not four letters!";
                return View(await SetTest(wordList, bymostrecent));
            }
            // get each of the four letters
            string c1 = test.Substring(0, 1);
            string c2 = test.Substring(1, 1);
            string c3 = test.Substring(2, 1);
            string c4 = test.Substring(3, 1);
            // test that they are on the list of useable letters and give specific feedback
            if (!_goodList[wordList-1].Contains(c1))
            {
                ViewData["Message"] = "First letter of " + test + " is not valid!";
                return View(await SetTest(wordList, bymostrecent));
            }
            if (!_goodList[wordList - 1].Contains(c2))
            {
                ViewData["Message"] = "Second letter of " + test + " is not valid!";
                return View(await SetTest(wordList, bymostrecent));
            }

            if (!_goodList[wordList - 1].Contains(c3))
            {
                ViewData["Message"] = "Third letter of " + test + " is not valid!";
                return View(await SetTest(wordList, bymostrecent));
            }
            if (!_goodList[wordList - 1].Contains(c4))
            {
                ViewData["Message"] = "Fourth letter of " + test + " is not valid!";
                return View(await SetTest(wordList, bymostrecent));
            }
            // check the list to see if this user already has it on their list of words
            var foundv = _db.Words
                .Where(p => p.Word == test)
                .Where(p => p.UserName == user)
                .Where(p => p.ListNum == wordList);

           if (foundv.Count() > 0)
            {
                ViewData["Message"] = test + " is already in your list!";
                return View(await SetTest(wordList, bymostrecent));
            }
           // add word to list
            Words x = new Words();
            x.Entered = DateTime.Now.ToUniversalTime();
            x.Word = test;
            x.UserName = user;
            x.ListNum = wordList;

            _db.Words.Add(x);
            await _db.SaveChangesAsync();
            // redisplay page        
            return View(await SetTest(wordList, bymostrecent));
        }

        /// <summary>
        /// Sets the model up for testing a word
        /// </summary>
        /// <param name="wordList">0..3</param>
        /// <param name="bymostrecent">how to sort</param>
        /// <returns>model for testing</returns>
        public async Task<ViewModels.CrazyWords.NewWordViewModel> SetTest(int wordList, bool bymostrecent)
        {

            ViewModels.CrazyWords.NewWordViewModel test = new ViewModels.CrazyWords.NewWordViewModel();
            test.myWord = new ViewModels.CrazyWords.MyWord();
            test.myList = new List<Words>();

            test.wordList = wordList;
            test.letters = _goodList[test.wordList - 1];
            test.letterarray = test.letters.ToCharArray();

            test.MostRecentFirst = bymostrecent;

            string user = User.Identity.Name;

            List<Words> stuff;
            if (bymostrecent)
            {
                stuff = await _db.Words
                    .Where(p => p.UserName == user && p.ListNum == wordList)
                    .OrderByDescending(p => p.Entered)
                    .ToListAsync();
            }
            else
            {
                stuff = await _db.Words
                    .Where(p => p.UserName == user && p.ListNum == wordList)
                    .OrderBy(p => p.Word)
                    .ToListAsync();
            }

            if ( stuff.Count() > 0)
            {
                test.myList = stuff;
            }

            return test;
        }

        /// <summary>
        /// Shows user hall of fame
        /// </summary>
        /// <param name="id">index of list of letters to use  0..3</param>
        /// <returns></returns>
        public ActionResult HallOfFame(int id)
        {
            int wordList = id;
            ViewData["wordList"] = wordList;
            List<HofCounts> cnts = new List<HofCounts>();  // create list of users/word counts
            // query the DB
            var query = _db.Words
                .Where(p => p.ListNum == wordList)
                .GroupBy(p => p.UserName);

            int key = 1;

            // for each user in list
            foreach (var userGroup in query)
            {
                HofCounts row = new HofCounts();
                row.user = userGroup.Key;
                int cnt = 0;
                // count their words
                foreach (Words unused in userGroup)
                {
                    cnt++;
                }
                row.count = cnt;
                row.ID = key++;
                cnts.Add(row);
            }
            // sort the list
            cnts.Sort();
            // display hall of fame
            return View(cnts);
        }

    }

    /// <summary>
    /// Utility class used to sort hall of fame users
    /// </summary>
    public class HofCounts : IComparable
    {
        public int CompareTo(object obj)
        {
            HofCounts other = obj as HofCounts;
            return other.count.CompareTo(this.count);
        }

        [Display(Name = "Word Count")]
        public int count { get; set; }

        [Display(Name = "User")]
        public string user { get; set; }

        [Key]
        public int ID { get; set; }
    }
}