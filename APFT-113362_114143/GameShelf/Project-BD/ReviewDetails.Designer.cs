namespace Project_BD
{
    partial class ReviewDetails
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblGameTitle = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblRating = new System.Windows.Forms.Label();
            this.lblHoursPlayed = new System.Windows.Forms.Label();
            this.txtReviewText = new System.Windows.Forms.TextBox();
            this.lblReviewDate = new System.Windows.Forms.Label();
            this.listReactions = new System.Windows.Forms.ListView();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnAddReaction = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblGameTitle
            // 
            this.lblGameTitle.AutoSize = true;
            this.lblGameTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblGameTitle.Location = new System.Drawing.Point(20, 20);
            this.lblGameTitle.Name = "lblGameTitle";
            this.lblGameTitle.Size = new System.Drawing.Size(110, 24);
            this.lblGameTitle.TabIndex = 0;
            this.lblGameTitle.Text = "Game Title";
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(22, 50);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(35, 13);
            this.lblUserName.TabIndex = 1;
            this.lblUserName.Text = "By: ...";
            // 
            // lblRating
            // 
            this.lblRating.AutoSize = true;
            this.lblRating.Location = new System.Drawing.Point(22, 70);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(41, 13);
            this.lblRating.TabIndex = 2;
            this.lblRating.Text = "Rating:";
            // 
            // lblHoursPlayed
            // 
            this.lblHoursPlayed.AutoSize = true;
            this.lblHoursPlayed.Location = new System.Drawing.Point(22, 90);
            this.lblHoursPlayed.Name = "lblHoursPlayed";
            this.lblHoursPlayed.Size = new System.Drawing.Size(76, 13);
            this.lblHoursPlayed.TabIndex = 3;
            this.lblHoursPlayed.Text = "Hours Played:";
            // 
            // txtReviewText
            // 
            this.txtReviewText.Location = new System.Drawing.Point(25, 120);
            this.txtReviewText.Multiline = true;
            this.txtReviewText.Name = "txtReviewText";
            this.txtReviewText.ReadOnly = true;
            this.txtReviewText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtReviewText.Size = new System.Drawing.Size(450, 150);
            this.txtReviewText.TabIndex = 4;
            // 
            // lblReviewDate
            // 
            this.lblReviewDate.AutoSize = true;
            this.lblReviewDate.Location = new System.Drawing.Point(350, 50);
            this.lblReviewDate.Name = "lblReviewDate";
            this.lblReviewDate.Size = new System.Drawing.Size(63, 13);
            this.lblReviewDate.TabIndex = 5;
            this.lblReviewDate.Text = "Posted on: ";
            // 
            // listReactions
            // 
            this.listReactions.HideSelection = false;
            this.listReactions.Location = new System.Drawing.Point(25, 290);
            this.listReactions.Name = "listReactions";
            this.listReactions.Size = new System.Drawing.Size(450, 150);
            this.listReactions.TabIndex = 6;
            this.listReactions.UseCompatibleStateImageBehavior = false;
            this.listReactions.View = System.Windows.Forms.View.Details;
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(25, 450);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(75, 23);
            this.btnBack.TabIndex = 7;
            this.btnBack.Text = "Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnAddReaction
            // 
            this.btnAddReaction.Location = new System.Drawing.Point(400, 450);
            this.btnAddReaction.Name = "btnAddReaction";
            this.btnAddReaction.Size = new System.Drawing.Size(75, 23);
            this.btnAddReaction.TabIndex = 8;
            this.btnAddReaction.Text = "Add Reaction";
            this.btnAddReaction.UseVisualStyleBackColor = true;
            this.btnAddReaction.Click += new System.EventHandler(this.btnAddReaction_Click);
            // 
            // ReviewDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 500);
            this.Controls.Add(this.btnAddReaction);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.listReactions);
            this.Controls.Add(this.lblReviewDate);
            this.Controls.Add(this.txtReviewText);
            this.Controls.Add(this.lblHoursPlayed);
            this.Controls.Add(this.lblRating);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.lblGameTitle);
            this.Name = "ReviewDetails";
            this.Text = "Review Details";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblGameTitle;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblRating;
        private System.Windows.Forms.Label lblHoursPlayed;
        private System.Windows.Forms.TextBox txtReviewText;
        private System.Windows.Forms.Label lblReviewDate;
        private System.Windows.Forms.ListView listReactions;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnAddReaction;
    }
}